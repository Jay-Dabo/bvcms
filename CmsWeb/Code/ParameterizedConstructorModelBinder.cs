using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CmsData;
using CmsData.Registration;
using UtilityExtensions;

namespace CmsWeb.Code
{
    public class ParameterizedConstructorModelBinder : DefaultModelBinder
    {
        protected override object CreateModel(ControllerContext controllerContext, ModelBindingContext bindingContext, Type modelType)
        {
            var constructors = modelType.GetConstructors();

            var hasParameterlessConstructor = constructors.Any(x => x.GetParameters().Count() == 0);

            var hostAndDbConstructor = modelType.GetConstructor(new Type[] { typeof(string), typeof(CMSDataContext) });
            var dbAndHostConstructor = modelType.GetConstructor(new Type[] { typeof(CMSDataContext), typeof(string) });
            var dbOnlyConstructor = modelType.GetConstructor(new Type[] { typeof(CMSDataContext) });

            var currentHost = Util.Host;
            var currentDb = DbUtil.Db;

            if (hostAndDbConstructor != null)
            {
                return hostAndDbConstructor.Invoke(new object[] { currentHost, currentDb });
            }

            if (dbAndHostConstructor != null)
            {
                return hostAndDbConstructor.Invoke(new object[] { currentDb, currentHost });
            }

            if (dbOnlyConstructor != null)
            {
                return hostAndDbConstructor.Invoke(new object[] { currentDb });
            }

            // prefer the parameterless version
            return base.CreateModel(controllerContext, bindingContext, modelType);
        }
    }
}
