using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ExpenseTracker.Biz.IServices
{
    public interface IViewRenderService
    {
        Task<string> RenderToStringAsync(string viewPath, object model);
    }
}
