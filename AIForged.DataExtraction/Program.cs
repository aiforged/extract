using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;

using AIForged;
using AIForged.API;

using DataAccess;
using Domain.Models;

namespace AIForged.DataExtractionTool
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            await RunExtraction(args);
        }

        private static async Task RunExtraction(string[] args)
        {
            Console.WriteLine($"OS: {Environment.OSVersion} 64bit {Environment.Is64BitOperatingSystem}");
            Console.WriteLine($"Net: {Environment.Version} 64bit {Environment.Is64BitProcess} Interactive: {Environment.UserInteractive}");

            var switchMappings = new Dictionary<string, string>()
            {
                { "-p", "ProjectId" },
                { "--p", "ProjectId" },
                { "-s", "ServiceId" },
                { "-u", "Username" },
                { "-pw", "Password" },
                { "-fd", "FromDate" },
                { "-td", "ToDate" },
                { "-fp", "FromPage" },
                { "-tp", "ToPage" },
                { "-ep", "EndPoint" },
                { "-b", "DocsPerBatch" }
            };

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())                
                .AddJsonFile("appsettings.json", optional: false)
                .AddCommandLine(args, switchMappings);

            IConfiguration config = builder.Build();

            var db = new DataExtractionContext(config.GetConnectionString("DataConnection"));
            DBInitialiser.Initialise(db);

            // get override values
            int projectId = Convert.ToInt32(config.GetSection("ProjectId").Value);
            int serviceId = Convert.ToInt32(config.GetSection("ServiceId").Value);

            //username and password used to register with AIForged 
            string username = config.GetSection("Username").Value;
            if (string.IsNullOrWhiteSpace(username))
            {
                Console.WriteLine($"Username cannot be blank");
                return;
            }

            string password = config.GetSection("Password").Value;
            if (string.IsNullOrWhiteSpace(password))
            {
                Console.WriteLine($"Password cannot be blank");
                return;
            }

            var endPoint = config.GetSection("EndPoint").Value;
            if (string.IsNullOrWhiteSpace(endPoint))
            {
                Console.WriteLine($"Endpoint cannot be blank");
                return;
            }

            DateTime fromDateTime = Convert.ToDateTime(config.GetSection("FromDate").Value);
            string toDate = config.GetSection("ToDate").Value;
            DateTime toDateTime = string.IsNullOrEmpty(toDate) ? DateTime.UtcNow : Convert.ToDateTime(toDate);

            int? fromPage = Convert.ToInt32(config.GetSection("FromPage").Value);
            int? toPage = Convert.ToInt32(config.GetSection("ToPage").Value);

            int numberDocsInBatch = Convert.ToInt32(config.GetSection("DocsPerBatch").Value);

            var cfg = new AIForged.API.Config(endPoint, username, password, "AIForged DataExtract");
            await cfg.Init(allowAutoRedirect: true);
            var ctx = new AIForged.API.Context(cfg);
            var user = await ctx.LoginAsync();

            var project = await ctx.GetProjectAsync(user.Id, projectId);
            var service = await ctx.GetServiceAsync(serviceId);

            Console.WriteLine($"User {user.DisplayName} Id: {user.Id}");
            Console.WriteLine($"Project {project.Name} Id: {project.Id}");
            Console.WriteLine($"Service {service.Name} Id: {service.Id}");

            for (int page = fromPage.Value; page < toPage.Value; page++)
            {
                var docsresp = await ctx.DocumentClient.GetExtendedAsync(user.Id,
                    project.Id,
                    service.Id,
                    UsageType.Outbox,
                    new List<DocumentStatus> { DocumentStatus.Processed },
                    null,
                    filename: null,// "11540090_20210928120034.579_X", 
                    null,
                    fromDateTime,
                    toDateTime,
                    null,
                    null,
                    pageNo: page,
                    pageSize: numberDocsInBatch,
                    SortField.Date,
                    SortDirection.Descending,
                    null, null, null, null, null, null, null);

                var pardefsresp = await ctx.ParamDefClient.GetHierachyAsync(project.Id, service.Id, false, false, false);
                ParameterDefViewModel pdHierarchy = pardefsresp.Result;

                Console.WriteLine($"Docs: {docsresp.Result.Count} Page: {page}");

                foreach (var doc in docsresp.Result)
                {
                    var dbdoc = db.Document.Local.FirstOrDefault(d => d.DocumentId == doc.Id);
                    dbdoc ??= db.Document.FirstOrDefault(d => d.DocumentId == doc.Id);
                    if (dbdoc != null)
                    {
                        Console.WriteLine($"Doc already processed {doc.Id} {doc.Filename}");
                    }
                    else
                    {
                        // Add document
                        dbdoc = new Document()
                        {
                            DocumentId = doc.Id,
                            ServiceId = doc.ServiceId,
                            ProjectId = doc.ProjectId,
                            Filename = doc.Filename,
                            ContentType = doc.ContentType,
                            DTC = doc.Dtc,
                            DTM = doc.Dtm,
                            MasterId = doc.MasterId,
                            Usage = doc.Usage,
                            Status = doc.Status,
                            ExternalId = doc.ExternalId,
                            CategoryId = doc.ClassId,
                            Result = doc.Result,
                            ResultId = doc.ResultId,
                            ResultIndex = doc.ResultIndex,
                            Comment = doc.Comment,
                        };
                        Console.WriteLine($"Create Doc {doc.Id} {doc.Filename}");
                        db.Document.Add(dbdoc);
                    }

                    var extarctresp = await ctx.ParametersClient.ExtractAsync(doc.Id);

                    var resultsExtract = extarctresp.Result
                        .Where(r => (r.Category == ParameterDefinitionCategory.Results || r.Category == ParameterDefinitionCategory.DataSet) &&
                                     r.Grouping != GroupingType.Word)
                        .ToList();

                    var resultsExtractRoot = resultsExtract.Where(r => r.ParentParamId == null).ToList();
                    foreach (var ext in resultsExtractRoot)
                    {
                        SaveMetaData(db, resultsExtract, ext, pdHierarchy, dbdoc);
                    }
                    db.SaveChanges();
                }
            }

            Console.WriteLine($"Data extraction Done!");
            Console.ReadKey();
        }


        static void SaveMetaData(DataExtractionContext db,
            List<DocumentExtraction> extractData,
            DocumentExtraction ext,
            ParameterDefViewModel pdHierarchy,
            Document doc)
        {
            // Check if has been added before as child
            switch (ext.Grouping)
            {
                case GroupingType.Table:
                case GroupingType.Anchor:
                    {
                        var (def, pdvm) = FindCreateDefinition(db, ext, pdHierarchy, null);
                        var docmd = FindCreateMetaData(db, ext, doc, def, null);
                        SaveChildren(db, extractData, ext, pdHierarchy, doc, def, docmd);
                    }
                    break;

                case GroupingType.Cluster:
                    {
                        var (def, pdvm) = FindCreateDefinition(db, ext, pdHierarchy, null);
                        var docmd = FindCreateMetaData(db, ext, doc, def, null);
                        SaveChildren(db, extractData, ext, pdHierarchy, doc, def, docmd);
                    }
                    break;

                //not supposed to happen at root
                case GroupingType.Column:
                    break;

                case GroupingType.Page:
                    {
                        var (def, pdvm) = FindCreateDefinition(db, ext, pdHierarchy, null);
                        var docmd = FindCreateMetaData(db, ext, doc, def, null);
                        SaveChildren(db, extractData, ext, pdHierarchy, doc, def, docmd);
                    }
                    break;

                case GroupingType.Field:
                case GroupingType.Form:
                case GroupingType.None:
                default:
                    {
                        var (def, pdvm) = FindCreateDefinition(db, ext, pdHierarchy, null);
                        var docmd = FindCreateMetaData(db, ext, doc, def, null);
                    }
                    break;
            }
        }

        static void SaveChildren(DataExtractionContext db,
            List<DocumentExtraction> extractData,
            DocumentExtraction ext,
            ParameterDefViewModel pdHierarchy,
            Document doc,
            Definition parentDef,
            Metadata parentMd)
        {
            // Get children
            var children = extractData.Where(e => e.ParentParamId == ext.ParamId).ToList();
            foreach (var child in children)
            {
                // Create def for child
                var (def, pdvm) = FindCreateDefinition(db, child, pdHierarchy, parentDef);
                if (parentDef.Grouping == GroupingType.Cluster &&
                    parentDef.Name == "LineItems" &&
                    child.Grouping == GroupingType.None)
                {
                    var docdet = FindCreateDetail(db, child, doc, def, parentMd);
                }
                else
                {                    
                    var docmd = FindCreateMetaData(db, child, doc, def, parentMd);
                }

                //Get the child's children and repeat
                //SaveChildren(db, extract, doc, def, child);
            }
        }

        static Metadata FindCreateMetaData(DataExtractionContext db,
            DocumentExtraction ext,
            Document doc,
            Definition def,
            Metadata parent)
        {
            var docmd = db.Metadata.Local.FirstOrDefault(md => md.ParamId == ext.ParamId);
            docmd ??= db.Metadata.FirstOrDefault(md => md.ParamId == ext.ParamId);
            if (docmd == null)
            {
                docmd = new Metadata()
                {
                    Definition = def,
                    Document = doc,
                    Parent = parent,
                    Value = ext.Value ?? ext.ParamValue,
                    ParamId = ext.ParamId,
                    ParamParentId = ext.ParentParamId,
                    ValueIndex = ext.ParamIndex,
                    //PageIndex = ??
                };
                db.Metadata.Add(docmd);
                //Console.WriteLine($"Create Metadata {docmd.Definition.Name} {docmd.ParamId} {docmd.Value}");
            }
            return docmd;
        }

        static Detail FindCreateDetail(DataExtractionContext db,
            DocumentExtraction ext,
            Document doc,
            Definition def,
            Metadata parent)
        {
            var docdet = db.Detail.Local.FirstOrDefault(md => md.ParamId == ext.ParamId);
            docdet ??= db.Detail.FirstOrDefault(md => md.ParamId == ext.ParamId);
            if (docdet == null)
            {
                docdet = new Detail()
                {
                    Definition = def,
                    Document = doc,
                    Parent = parent,
                    Value = ext.Value ?? ext.ParamValue,
                    ParamId = ext.ParamId,
                    ParamParentId = ext.ParentParamId,
                    ValueIndex = ext.ParamIndex,
                    //PageIndex = ??
                };
                db.Detail.Add(docdet);
                //Console.WriteLine($"Create Detail {docdet.Definition.Name} {docdet.ParamId} {docdet.Value}");
            }
            return docdet;
        }

        static (Definition def, ParameterDefViewModel pdvm) FindCreateDefinition(DataExtractionContext db,
            DocumentExtraction ext,
            ParameterDefViewModel pdHierarchy,
            Definition parent)
        {
            ParameterDefViewModel pdvm = AIForged.Tools.GetChild(pdHierarchy, ext.Id);

            Definition def = db.Defintion.Local.FirstOrDefault(def => def.ExternalId == ext.Id);
            def ??= db.Defintion.FirstOrDefault(def => def.ExternalId == ext.Id);
            if (def == null)
            {
                string name = LarcAI.Core.Utilities.MakeFieldName($"{parent?.Name}_{ext.Name}".Trim('_').Trim());
                def = new Definition()
                {
                    DataType = (Domain.Enum.Enums.ValueType)((int)ext.ValueType),
                    ExternalId = ext.Id,
                    ExternalParentId = ext.ParentId,
                    Name = name,
                    Index = pdvm.Index,
                    Category = pdvm.Category ?? ParameterDefinitionCategory.None,
                    Grouping = pdvm.Grouping,
                    Parent = parent
                };
                db.Defintion.Add(def);

                Console.WriteLine($"Creating Definition {def.ExternalId} {def.Name}");
            }   
            return (def, pdvm);
        }

    }

}
