SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[InvoiceExpenses]
AS
SELECT 
	pivot_table.ProjectId, 
	pivot_table.ServiceId, 
	pivot_table.DocumentId,
	pivot_table.Filename,
	pivot_table.Status,
	pivot_table.Usage, 
	pivot_table.DTC,
	pivot_table.MDIndex, 
	COALESCE(pivot_table.[ExpenseTable_LineIndex], '') AS LineIndex,
	COALESCE(pivot_table.[ExpenseTable_Amount], '') AS Amount,
	COALESCE(pivot_table.[ExpenseTable_Basic], '') AS Basic,
	COALESCE(pivot_table.[ExpenseTable_Consumption], '') AS Consumption,
	COALESCE(pivot_table.[ExpenseTable_DailyAverage], '') AS DailyAverage,
	COALESCE(pivot_table.[ExpenseTable_ExpenseType], '') AS ExpenseType,
	COALESCE(pivot_table.[ExpenseTable_MeterNo], '') AS MeterNo,
	COALESCE(pivot_table.[ExpenseTable_PayableAmount], '') AS PayableAmount,
	COALESCE(pivot_table.[ExpenseTable_ReadingDate], '') AS ReadingDate,
	COALESCE(pivot_table.[ExpenseTable_ReadingNew], '') AS ReadingNew,
	COALESCE(pivot_table.[ExpenseTable_ReadingOld], '') AS ReadingOld,
	COALESCE(pivot_table.[ExpenseTable_ReadingType], '') AS ReadingType
	FROM            
	(
		SELECT      doc.ProjectId, doc.ServiceId, doc.DocumentId, doc.Filename, doc.Status, doc.Usage, doc.DTC, mdef.Name MDName, m.[ValueIndex] MDIndex, m.Value MDValue
            FROM        [aiforged-extract].[dbo].[Document] doc JOIN
                        [aiforged-extract].[dbo].[Metadata] m ON m.DocumentId = doc.Id JOIN
                        [aiforged-extract].[dbo].[Defintion] mdef ON m.DefinitionId = mdef.Id where mdef.Name like '%ExpenseTable%'
	) t
	PIVOT
	( 
		max(t.MDValue) for t.MDName in 
					(
						[ExpenseTable_Amount],
						[ExpenseTable_Basic],
						[ExpenseTable_Consumption],
						[ExpenseTable_DailyAverage],
						[ExpenseTable_ExpenseType],
						[ExpenseTable_MeterNo],
						[ExpenseTable_PayableAmount],
						[ExpenseTable_ReadingDate],
						[ExpenseTable_ReadingNew],
						[ExpenseTable_ReadingOld],
						[ExpenseTable_ReadingType],
						[ExpenseTable_LineIndex]
					)   
	) AS pivot_table
	
GO