﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreBot.Store;

namespace CoreBot.Extensions
{
    public static class ApiCallerExtensions
    {
        public static string ToQueryListParameter<T>(this List<T> tList) where T : IIdentifiable
        {
            if (tList.Count == 0) throw new EmptyParameterListException("List cannot be empty");

            string list = "[";
            int count = 0;

            foreach (T element in tList)
            {
                list += element.Id;
                count++;
                list += (count != tList.Count) ? "," : "";
            }

            return (list + "]");
        }

        public static string ToFilterParameterList(this List<string> list)
        {
            if (list.Count == 0) throw new EmptyParameterListException("List cannot be empty");

            string parameter = "";

            foreach (string element in list)
            {
                parameter += "&filter[name]=%[" + element + "]%";
            }

            return parameter;
        }
    }

    public class EmptyParameterListException : Exception
    {
        public EmptyParameterListException(string message) : base(message)
        {

        }
    }
}
