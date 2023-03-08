SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[InvoiceUtilities]
AS
SELECT * FROM   
(
   select doc.ProjectId, doc.ServiceId, doc.DocumentId as DocumentId, doc.Filename, doc.DTC, def.Name, m.Value
     from [aiforged-extract].[dbo].[Document] doc
     join [aiforged-extract].[dbo].[Metadata] m on m.DocumentId = doc.Id
     join [aiforged-extract].[dbo].[Defintion] def on m.DefinitionId = def.Id
) t 
PIVOT( max(t.Value) for t.Name in (
	Invoice_Layout_ElecAmount,
    Invoice_Layout_ElecAmountResults,
    Invoice_Layout_ElecReading,
    Invoice_Layout_ElecReadingResults,
    Invoice_Layout_ElectricityPeriod,
    Invoice_Layout_ElecUsage,
    Invoice_Layout_ElecUsageResults,
    Invoice_Layout_RefuseAmount,
    Invoice_Layout_RefuseAmountResults,
    Invoice_Layout_RefuseReading,
    Invoice_Layout_RefuseReadingResults,
    Invoice_Layout_RefuseUsage,
    Invoice_Layout_RefuseUsageResults,
    Invoice_Layout_SewerageAmount,
    Invoice_Layout_SewerageAmountResults,
    Invoice_Layout_SewerageReading,
    Invoice_Layout_SewerageReadingResults,
    Invoice_Layout_SewerageUsage,
    Invoice_Layout_SewerageUsageResults,
    Invoice_Layout_WaterAmount,
    Invoice_Layout_WaterAmountResults,
    Invoice_Layout_WaterReading,
    Invoice_Layout_WaterReadingResults,
    Invoice_Layout_WaterUsage,
    Invoice_Layout_WaterUsageResults
    )								   
) AS pivot_table;

GO
