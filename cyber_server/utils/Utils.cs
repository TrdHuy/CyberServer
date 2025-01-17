﻿using cyber_server.models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace cyber_server.utils
{
    public static class Utils
    {
        public static DataTable ToDataTable<T>(IEnumerable<T> data)
        {
            PropertyDescriptorCollection properties =
                TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new DataTable();
            foreach (PropertyDescriptor prop in properties)
                table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            foreach (T item in data)
            {
                DataRow row = table.NewRow();
                foreach (PropertyDescriptor prop in properties)
                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                table.Rows.Add(row);
            }
            return table;
        }

        public static async void DelayLoadAsync<T>(this DbSet<T> source, int delay = 100)
            where T : class
        {
            await Task.Delay(delay);
            await source.LoadAsync();
        }

        public static List<TSource> Clone<TSource>(this IQueryable<TSource> source) where TSource : ICloneable
        {
            var cloneRes = new List<TSource>(); 
            foreach(var item in source)
            {
                var cloneItem = (TSource)item.Clone();
                cloneRes.Add(cloneItem);
            }
            return cloneRes;
        }

        public static List<TSource> JsonClone<TSource>(this IQueryable<TSource> source) where TSource : IJsonCloneable
        {
            var cloneRes = new List<TSource>();
            foreach (var item in source)
            {
                var cloneItem = (TSource)item.JsonClone();
                cloneRes.Add(cloneItem);
            }
            return cloneRes;
        }
    }
}
