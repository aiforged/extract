SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[InvoiceMetaData]
AS
SELECT * FROM   
(
   select doc.ProjectId, doc.ServiceId, doc.DocumentId as DocId, concat('aiforged://doc/', doc.MasterId) Link, doc.Filename, doc.DTC, def.Name, m.Value
     from [aiforged-extract].[dbo].[Document] doc
     join [aiforged-extract].[dbo].[Metadata] m on m.DocumentId = doc.Id
     join [aiforged-extract].[dbo].[Defintion] def on m.DefinitionId = def.Id
) t 
PIVOT( max(t.Value) for t.Name in (
								   [Invoice_Layout_InvoiceNumber],[Invoice_Layout_InvoiceDate],
								   [Invoice_Layout_PropertyName],[Invoice_Layout_PropertyUnit],[Invoice_Layout_TenantName],[Invoice_Layout_Reference],
								   [SAP_Branch_Name],[SAP_Branch_Number],[SAP_CoCode],[SAP_Vendor_Number],[SAP_Payment_Reference],[VAT_Code],[Expense_Type],
								   [Invoice_Layout_BankName],[Invoice_Layout_BankAccount],[Invoice_Layout_BankCode],
                                   [Vendor_VendorId],[Vendor_Name],[Vendor_Street],[Vendor_ZIP],[Vendor_City],[Vendor_State],[Vendor_Country],[Vendor_Address],
								   [BU_BUId],[BU_Name],[BU_Street],[BU_City],[BU_State],[BU_Country],[BU_Address],								   
								   [Invoice_Layout_TotalNetAmount],[Invoice_Layout_TotalTaxAmount],[Invoice_Layout_Total],[Invoice_Layout_TotalValidation],
								   [Amounts_TotalNetAmount],   								   
								   [Invoice_Layout_SMKey],
								   [Invoice_Layout_VendorVAT],
								   [Invoice_Layout_VendorReg]
								   )
								   
								   ) AS pivot_table;
GO