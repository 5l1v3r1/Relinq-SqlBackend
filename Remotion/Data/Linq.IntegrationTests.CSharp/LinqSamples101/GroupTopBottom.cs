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

namespace Remotion.Data.Linq.IntegrationTests.CSharp.LinqSamples101
{
  internal class GroupTopBottom : Executor
  {
    //This sample uses Take to select the first 5 Employees hired.")]
    public void LinqToSqlTop01 ()
    {
      var q = (
                  from e in db.Employees
                  orderby e.HireDate
                  select e)
          .Take (5);

      serializer.Serialize (q);
    }

    //This sample uses Skip to select all but the 10 most expensive Products.")]
    public void LinqToSqlTop02 ()
    {
      var q = (
                  from p in db.Products
                  orderby p.UnitPrice descending
                  select p)
          .Skip (10);

      serializer.Serialize (q);
    }
  }
}