﻿using GCGInterviews.Models;
using GCGInterviews.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCGInterviews.Interfaces
{
    public interface IPublishProductService
    {
        PublishProductListModel GetPublishProductList(NameValueCollection expands, FilterCollection filters, NameValueCollection sorts, NameValueCollection page);

    }
}
