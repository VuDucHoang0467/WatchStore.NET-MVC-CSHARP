using WebDongHo.Models.Momo;
using WebDongHo.Models.Orders;

namespace WebDongHo.Services;

public interface IMomoService
{
    Task<MomoCreatePaymentResponseModel> CreatePaymentAsync(OrderInfoModel model);
    MomoExecuteResponseModel PaymentExecuteAsync(IQueryCollection collection);
}