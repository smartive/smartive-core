using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Smartive.Core.Debug.Mvc.Routing
{
    /// <summary>
    /// RouteInformation class that provides helper accessors for the razor view.
    /// </summary>
    public class RouteInformation
    {
        private readonly ActionDescriptor _descriptor;

        /// <summary>
        /// Returns the HTTP-METHOD for the given route,
        /// "All" if the <see cref="RouteAttribute"/> is used or "Unknown" if no
        /// actual method is found.
        /// </summary>
        public string Method
        {
            get
            {
                if (_descriptor.ActionConstraints == null)
                {
                    return "All";
                }

                if (_descriptor.ActionConstraints.Any(
                    constraint => constraint is HttpMethodActionConstraint))
                {
                    return (_descriptor.ActionConstraints.First(
                            constraint => constraint is HttpMethodActionConstraint) as HttpMethodActionConstraint)
                        ?.HttpMethods.First();
                }

                return "Unknown";
            }
        }

        /// <summary>
        /// The bootstrap specific css class for the badge. Depends on the return
        /// value of <see cref="Method"/>.
        /// </summary>
        public string MethodCss
        {
            get
            {
                switch (Method)
                {
                    case "GET":
                        return "success";
                    case "DELETE":
                        return "danger";
                    case "All":
                        return "primary";
                    case "Unknown":
                        return "warning";
                    default:
                        return "info";
                }
            }
        }

        /// <summary>
        /// The mvc-area (if any) of the route.
        /// </summary>
        public string Area => _descriptor.RouteValues.ContainsKey("area") ? _descriptor.RouteValues["area"] : null;

        /// <summary>
        /// The given action path of the route. Returns the view engine path if the descriptor
        /// is a <see cref="PageActionDescriptor"/>, the controller/action path if the descriptor
        /// is a <see cref="ControllerActionDescriptor"/> or the template name if none of the
        /// above match.
        /// </summary>
        public string Path
        {
            get
            {
                switch (_descriptor)
                {
                    case PageActionDescriptor pageDescriptor:
                        return pageDescriptor.ViewEnginePath;
                    case ControllerActionDescriptor controllerDescriptor:
                        return $"/{controllerDescriptor.ControllerName}/${controllerDescriptor.ActionName}";
                    default:
                        return Template;
                }
            }
        }

        /// <summary>
        /// The template path of the descriptor.
        /// </summary>
        public string Template => $"/{_descriptor.AttributeRouteInfo?.Template}";

        /// <summary>
        /// Invocation action of the given descriptor. Contains additional
        /// information about the invocation that is used to deliver the route.
        /// </summary>
        public string Invocation
        {
            get
            {
                var sb = new StringBuilder($"({_descriptor.DisplayName})");

                switch (_descriptor)
                {
                    case PageActionDescriptor pageDescriptor:
                        sb.Insert(0, $"{pageDescriptor.RelativePath} ");
                        break;
                    case ControllerActionDescriptor controllerDescriptor:
                        sb.Insert(
                            0,
                            $"{controllerDescriptor.ControllerName}Controller.{controllerDescriptor.ActionName} ");
                        break;
                }

                return sb.ToString();
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="descriptor">The action descriptor that should be used.</param>
        public RouteInformation(ActionDescriptor descriptor)
        {
            _descriptor = descriptor;
        }
    }
}
