SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[InvoiceLineItems]
AS
SELECT * FROM   
(
   select doc.ProjectId, doc.ServiceId, doc.DocumentId, doc.Filename, doc.Status, doc.Usage, doc.DTC, mdef.Name MDName, m.[ValueIndex] MDIndex, m.Value MDValue, def.Name, d.Value
     from [aiforged-extract].[dbo].[Document] doc
	 join [aiforged-extract].[dbo].[Metadata] m on m.DocumentId = doc.Id
	 join [aiforged-extract].[dbo].[Defintion] mdef on m.DefinitionId = mdef.Id
     join [aiforged-extract].[dbo].[Detail] d on d.DocumentId = doc.Id and d.ParentId = m.Id
     join [aiforged-extract].[dbo].[Defintion] def on d.DefinitionId = def.Id
) t 
PIVOT( max(t.Value) for t.Name in ([LineItems_OrderDate],[LineItems_ArticleNumber], 
                                   [LineItems_Description], 
								   [LineItems_Quantity], [LineItems_UnitPrice],
								   [LineItems_TotalPriceNetto], [LineItems_VATPercentage], [LineItems_VATValue], [LineItems_TotalPriceBrutto],
								   [LineItems_IsValid]) ) AS pivot_table;
GO