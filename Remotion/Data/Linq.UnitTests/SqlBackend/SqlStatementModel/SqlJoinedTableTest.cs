// This file is part of the re-motion Core Framework (www.re-motion.org)
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
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Data.Linq.UnitTests.TestDomain;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.UnitTests.SqlBackend.SqlStatementModel
{
  [TestFixture]
  public class SqlJoinedTableTest
  {
    [Test]
    public void SameType ()
    {
      var oldJoinInfo = new JoinedTableSource (typeof (Kitchen).GetProperty ("Cook"));
      var sqlJoinedTable = new SqlJoinedTable (oldJoinInfo);
      var newJoinInfo = new JoinedTableSource (typeof (Cook).GetProperty ("Substitution"));

      sqlJoinedTable.JoinInfo = newJoinInfo;

      Assert.That (sqlJoinedTable.JoinInfo.ItemType, Is.EqualTo (newJoinInfo.ItemType));
    }

    [Test]
    [ExpectedException (typeof (ArgumentTypeException))]
    public void DifferentType ()
    {
      var oldJoinInfo = new JoinedTableSource (typeof (Kitchen).GetProperty ("Cook"));
      var sqlJoinedTable = new SqlJoinedTable (oldJoinInfo);
      var newJoinInfo = new JoinedTableSource (typeof (Cook).GetProperty ("FirstName"));

      sqlJoinedTable.JoinInfo = newJoinInfo;
    }

    [Test]
    public void GetOrAddJoin_NewEntry ()
    {
      var memberInfo = typeof (Cook).GetProperty ("FirstName");
      var tableSource = new JoinedTableSource (memberInfo);
      var sqlJoinedTable = new SqlJoinedTable(tableSource);
      
      var table = sqlJoinedTable.GetOrAddJoin (memberInfo, tableSource);

      Assert.That (table.JoinInfo, Is.SameAs (tableSource));
    }

    [Test]
    public void GetOrddJoin_GetEntry_WithNewTableSourceForMember ()
    {
      var memberInfo = typeof (Cook).GetProperty ("FirstName");
      var expectedTableSource = new JoinedTableSource (memberInfo);
      var newTableSource = new JoinedTableSource (memberInfo);
      var sqlJoinedTable = new SqlJoinedTable (expectedTableSource);

      sqlJoinedTable.GetOrAddJoin (memberInfo, expectedTableSource);

      Assert.That (sqlJoinedTable.GetOrAddJoin (memberInfo, newTableSource).JoinInfo, Is.SameAs (expectedTableSource));
    }

    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "Type mismatch between String and Int32.")]
    [Test]
    public void GetOrAddJoin_ThrowsException ()
    {
      var memberInfo = typeof (Cook).GetProperty ("FirstName");
      var tableSource = new JoinedTableSource (typeof (Cook).GetProperty ("ID"));

      var sqlJoinedTable = new SqlJoinedTable (tableSource);
      
      sqlJoinedTable.GetOrAddJoin (memberInfo, tableSource);
    }

  }
}