using System;
using System.Collections.Generic;

namespace sample2.models
{
    public class ProductModel
    {
        public string Pr_Expiry_Date { get; set; }
        public string Pr_Name { get; set; }
        public string Pr_Description { get; set; }
        public byte[] Pr_image { get; set; }
        public string Pr_HSN { get; set; }
        public string Pr_Category { get; set; }
        public string Pr_Buying_Price { get; set; }
        public string Pr_SGST { get; set; }
        public string Pr_CGST { get; set; }
        public string Pr_IGST { get; set; }
        public string Pr_Selling_Price { get; set; }
        public string Pr_Min_Qty { get; set; }
        public string Pr_Notify_Before { get; set; }
    }

    public class ProductTransactionModel
    { 
        public int CTT_Row_No { get; set; }
        public int CTT_Col_No { get; set; }
        public string CTT_Product_Name { get; set; }
        public int CTT_Transaction_Qty { get; set; }
        public string CTT_Transaction_Type { get; set; }
        public int CTT_Transaction_Price { get; set; }
        public double CTT_SGST { get; set; }
        public double CTT_CGST { get; set; }
        public double CTT_IGST { get; set; }
        public string CTT_DateTime { get; set; }
        public string CTT_Payment_Type { get; set; }
        public string CTT_Invoice_Req_No { get; set; }
        public int CTT_Opening_Stock { get; set; }
        public int CTT_Closing_Stock { get; set; }
        public int CTT_Transaction_Total_Amount { get; set; }
        public string CTT_Remarks { get; set; }
        public string CTT_Extras { get; set; }
        public string CTT_Username { get; set; }
        public string CTT_ExpiryDate { get; set; }
        public string CTT_Status { get; set; }
        public string CTT_Mode { get; set; }


    }


    public class CurrencyTransactionModel
    {
        
        public string Cr_Denomination { get; set; }
        public int Cr_Transaction_Qty { get; set; }
        public string Cr_Transaction_Type { get; set; }
        public string Cr_Equipment_Used { get; set; }
        public string Cr_DateTime { get; set; }
        public string Cr_Invoice_Req_No { get; set; }
        public int Cr_Opening_Balance_Qty { get; set; }
        public int Cr_Closing_Balance_Qty { get; set; }
        public string Cr_Status { get; set; }
        public string Cr_Remarks { get; set; }
        public string Cr_User { get; set; }
    }


    public class TaxModel
    {
        public double Tax_Rate { get; set; }
        public double Total_Value { get; set; }
    }

    public class CategoryModel
    {
        public string CT_Name { get; set; }
        public byte[] CT_Image { get; set; }
    }



        public class BillProduct
    {
        public string Pr_Name { get; set; }
        public string Pr_HSN { get; set; }
        public double Pr_Rate { get; set; }
        public double Pr_SGST { get; set; }
        public double Pr_CGST { get; set; }
        public double Pr_IGST { get; set; }
        public double Pr_Final_Rate { get; set; }
        public int Pr_Qty { get; set; }
    }

    public class product_count_ledger
    {
        public string Pr_Name { get; set; }
        public int Pr_Qty { get; set; }
    }

        public class LogModel
    {
        public int LT_Event_code { get; set; }
        public string LT_eventState { get; set; }
        public string LT_username { get; set; }
        public string LT_usertype { get; set; }
    }


    public class NoteAcceptorModel
    {
        public int NA_Denomination { get; set; }
        public int NA_Quantity { get; set; }
        public DateTime NA_DateTime { get; set; }
        public int NA_Amount { get; set; }
        public string NA_TransactionType { get; set; }
        public string NA_TransactionID { get; set; }
        public int NA_Balance { get; set; }
    }

    public class DenominationRecords
    {
        public List<int> Balance_Nos { get; set; }
        public int Total_Balance { get; set; }
    }

    public class Card_Response
    {
        public string BillInvoiceNumber { get; set; }
        public string DATE { get; set; }
        public string TIME { get; set; }
        public string TID { get; set; }
        public string MID { get; set; }
        public string BatchNO { get; set; }
        public string TxnID { get; set; }
        public string CardNO { get; set; }
        public string CardType { get; set; }
        public string ApprCode { get; set; }
        public string RRNo { get; set; }
        public string AMOUNT { get; set; }
        public string CardHolderName { get; set; }
    }
}

