namespace MerchantTransactionProcessing.Exceptions
{
    public class ResourceNotFound: Exception
    {
        public ResourceNotFound(object id,string? resourceName = "Resource"):base($"{resourceName} with id {id} was not found")
        {
            
        }
    }
}
