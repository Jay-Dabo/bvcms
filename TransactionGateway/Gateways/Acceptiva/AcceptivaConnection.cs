﻿using CmsData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TransactionGateway.Gateways.Acceptiva
{
    public class AcceptivaConnection
    {
        private ApiClient _client;
        private string _accessToken;
        private string _refreshToken;
        private CMSDataContext db;

        public AcceptivaConnection(CMSDataContext db_context)
        {
            _accessToken = "";
            _refreshToken = "";
            db = db_context;
        }
    }

    
}
