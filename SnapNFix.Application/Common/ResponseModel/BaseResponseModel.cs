namespace SnapNFix.Application.Common.ResponseModel;

public class BaseResponseModel
{
    public string Message { get; set; }
    public ICollection<ErrorResponseModel> ErrorList { get; set; }

    public BaseResponseModel(string message)
    {
        Message = message;
        ErrorList = new List<ErrorResponseModel>();
    }

    public BaseResponseModel(string message, IList<ErrorResponseModel> errorList)
    {
        Message = message;
        ErrorList = errorList;
    }
}

public class ErrorResponseModel
{
    public string PropertyName { get; set; }
    public string Message { get; set; }

    public static ErrorResponseModel Create(string propertyName, string message) =>
        new() { PropertyName = propertyName, Message = message };
}