using ExpenseTracker.Biz.IServices;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Builder.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace ExpenseTracker.Biz.Services
{
    public class ViewRenderService : IViewRenderService
    {
        private readonly IRazorViewEngine _razorViewEngine;
        private readonly ITempDataProvider _tempDataProvider;
        private readonly IServiceProvider _serviceProvider;
        private readonly HttpContext _context;

        public ViewRenderService(IRazorViewEngine razorViewEngine,
            ITempDataProvider tempDataProvider,
            IServiceProvider serviceProvider, IHttpContextAccessor accessor)
        {
            _razorViewEngine = razorViewEngine;
            _tempDataProvider = tempDataProvider;
            _serviceProvider = serviceProvider;
            _context = accessor.HttpContext;
        }

        public async Task<string> RenderToStringAsync(string viewPath, object model)
        {
            try
            {
                //var httpContext = new DefaultHttpContext { RequestServices = _serviceProvider };
                //var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
                var actionContext = GetActionContext();

                using (var stringwriter = new StringWriter())
                {
                    var viewResult = _razorViewEngine.FindView(actionContext, viewPath, false);

                    if (viewResult.View == null)
                    {
                        throw new ArgumentNullException($"{viewPath} does not match any available view");
                    }

                    var viewDictionary = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
                    {
                        Model = model
                    };

                    var viewContext = new ViewContext(
                        actionContext,
                        viewResult.View,
                        viewDictionary,
                        new TempDataDictionary(actionContext.HttpContext, _tempDataProvider),
                        stringwriter,
                        new HtmlHelperOptions()
                    );
                    //viewContext.RouteData = _context.GetRouteData();

                    await viewResult.View.RenderAsync(viewContext);
                    return stringwriter.ToString();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return "";
            }
        }

        private ActionContext GetActionContext()
        {
            var httpContext = new DefaultHttpContext { RequestServices = _serviceProvider };
            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());

            MapRoutes(actionContext);

            return actionContext;
        }

        private void MapRoutes(ActionContext actionContext)
        {
            var routes = new RouteBuilder(new ApplicationBuilder(_serviceProvider))
            {
                DefaultHandler = new DefaultHandler()
            };
            routes.MapRoute(
                name:"default",
                template: "{controller=Home}/{action=Index}/{id?}"
            );
            actionContext.RouteData.Routers.Add(routes.Build());
        }

        /// <summary>
        /// Not actually used, but needed to get past the validation checks in routes.MapRoute
        /// </summary>
        private class DefaultHandler : IRouter
        {
            public VirtualPathData GetVirtualPath(VirtualPathContext context) => null;
            public Task RouteAsync(RouteContext context) => Task.CompletedTask;
        }

    }
}
