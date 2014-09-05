﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TasksWeb.Models
{
    public class TaskViewModel
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public List<Task> Tasks { get; set; }
    }
}