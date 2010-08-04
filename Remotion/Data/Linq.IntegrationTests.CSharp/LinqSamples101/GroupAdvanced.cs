﻿// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// as published by the Free Software Foundation; either version 2.1 of the 
// License, or (at your option) any later version.
// 
// re-motion is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-motion; if not, see http://www.gnu.org/licenses.
// 
using System;
using System.Linq;
using System.Linq.Expressions;
using Remotion.Data.Linq.IntegrationTests.TestDomain.Northwind;

namespace Remotion.Data.Linq.IntegrationTests.CSharp.LinqSamples101
{
  internal class GroupAdvanced:Executor
  {
    //This sample builds a query dynamically to return the contact name of each customer. 
    //The GetCommand method is used to get the generated T-SQL of the query.")]
    public void LinqToSqlAdvanced01 ()
    {
      ParameterExpression param = Expression.Parameter (typeof (Customer), "c");
      Expression selector = Expression.Property (param, typeof (Customer).GetProperty ("ContactName"));
      Expression pred = Expression.Lambda (selector, param);

      IQueryable<Customer> custs = db.Customers;
      Expression expr = Expression.Call (typeof (Queryable), "Select", new Type[] { typeof (Customer), typeof (string) }, Expression.Constant (custs), pred);
      IQueryable<string> query = db.Customers.AsQueryable ().Provider.CreateQuery<string> (expr);

      System.Data.Common.DbCommand cmd = db.GetCommand (query);
      serializer.Serialize ("Generated T-SQL:");
      serializer.Serialize (cmd.CommandText);
      serializer.Serialize (Environment.NewLine);

      serializer.Serialize (query);
    }

    //This sample builds a query dynamically to filter for Customers in London.")]
    public void LinqToSqlAdvanced02 ()
    {
      IQueryable<Customer> custs = db.Customers;
      ParameterExpression param = Expression.Parameter (typeof (Customer), "c");
      Expression right = Expression.Constant ("London");
      Expression left = Expression.Property (param, typeof (Customer).GetProperty ("City"));
      Expression filter = Expression.Equal (left, right);
      Expression pred = Expression.Lambda (filter, param);

      Expression expr = Expression.Call (typeof (Queryable), "Where", new Type[] { typeof (Customer) }, Expression.Constant (custs), pred);
      IQueryable<Customer> query = db.Customers.AsQueryable ().Provider.CreateQuery<Customer> (expr);
      serializer.Serialize (query);
    }

    //This sample builds a query dynamically to filter for Customers in London
    //and order them by ContactName.")]
    public void LinqToSqlAdvanced03 ()
    {
      ParameterExpression param = Expression.Parameter (typeof (Customer), "c");

      Expression left = Expression.Property (param, typeof (Customer).GetProperty ("City"));
      Expression right = Expression.Constant ("London");
      Expression filter = Expression.Equal (left, right);
      Expression pred = Expression.Lambda (filter, param);

      IQueryable custs = db.Customers;

      Expression expr = Expression.Call (typeof (Queryable), "Where",
          new Type[] { typeof (Customer) }, Expression.Constant (custs), pred);

      expr = Expression.Call (typeof (Queryable), "OrderBy",
          new Type[] { typeof (Customer), typeof (string) }, custs.Expression, Expression.Lambda (Expression.Property (param, "ContactName"), param));


      IQueryable<Customer> query = db.Customers.AsQueryable ().Provider.CreateQuery<Customer> (expr);

      serializer.Serialize (query);
    }

    //This sample dynamically builds a Union to return a sequence of all countries where either 
    //a customer or an employee live.")]
    public void LinqToSqlAdvanced04 ()
    {
      IQueryable<Customer> custs = db.Customers;
      ParameterExpression param1 = Expression.Parameter (typeof (Customer), "e");
      Expression left1 = Expression.Property (param1, typeof (Customer).GetProperty ("City"));
      Expression pred1 = Expression.Lambda (left1, param1);

      IQueryable<Employee> employees = db.Employees;
      ParameterExpression param2 = Expression.Parameter (typeof (Employee), "c");
      Expression left2 = Expression.Property (param2, typeof (Employee).GetProperty ("City"));
      Expression pred2 = Expression.Lambda (left2, param2);

      Expression expr1 = Expression.Call (typeof (Queryable), "Select", new Type[] { typeof (Customer), typeof (string) }, Expression.Constant (custs), pred1);
      Expression expr2 = Expression.Call (typeof (Queryable), "Select", new Type[] { typeof (Employee), typeof (string) }, Expression.Constant (employees), pred2);

      IQueryable<string> q1 = db.Customers.AsQueryable ().Provider.CreateQuery<string> (expr1);
      IQueryable<string> q2 = db.Employees.AsQueryable ().Provider.CreateQuery<string> (expr2);

      var q3 = q1.Union (q2);

      serializer.Serialize (q3);
    }

    //This sample uses orderbyDescending and Take to return the 
    //discontinued products of the top 10 most expensive products.")]
    public void LinqToSqlAdvanced06 ()
    {
      var prods = from p in db.Products.OrderByDescending (p => p.UnitPrice).Take (10)
                  where p.Discontinued
                  select p;

      serializer.Serialize (prods);
    }
  }
}