using Microsoft.VisualBasic;
using Constants = SnapNFix.Application.Utilities.Constants;

namespace SnapNFix.Application.Common.ResponseModel;

public class GenericResponseModel<TResponse> : BaseResponseModel
{
    public TResponse Data { get; set; }

    public GenericResponseModel(string message, IList<ErrorResponseModel> errorList) : base(message, errorList)
    {
        Type t = typeof(TResponse);
        if (t.GetConstructor(Type.EmptyTypes) != null)
        {
            Data = Activator.CreateInstance<TResponse>();
        }
    }

    public GenericResponseModel(string message, TResponse data) : base(message) => Data = data;


    public static GenericResponseModel<TResponse> Success(TResponse data) => new(Constants.SuccessMessage, data);

    public static GenericResponseModel<TResponse> Failure(string message) =>
        new(message, new List<ErrorResponseModel>());

    public static GenericResponseModel<TResponse> Failure(string message, IList<ErrorResponseModel> errorList) =>
        new(message, errorList);
}