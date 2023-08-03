global using System;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.AspNetCore.Mvc.Filters;
global using Microsoft.EntityFrameworkCore;


global using JaegerWebAPI.Services;
global using JaegerWebAPI.Middleware;
global using JeagerWebAPI.Filter;

global using OpenTracing;
global using OpenTracing.Propagation;
global using OpenTracing.Util;


global using Jaeger;
global using Jaeger.Samplers;