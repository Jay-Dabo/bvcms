﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace CMSWeb.Reports
{
    public partial class Reports : System.Web.UI.MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            cssbundle.Text = ViewExtensions2.StandardCss();
        }
    }
}
