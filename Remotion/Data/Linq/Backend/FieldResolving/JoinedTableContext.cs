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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reflection;
using Remotion.Data.Linq.Backend.DataObjectModel;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.Backend.FieldResolving
{
  public class JoinedTableContext
  {
    private readonly IDatabaseInfo _databaseInfo;
    private readonly Dictionary<FromClauseBase, IColumnSource> _columnSources = new Dictionary<FromClauseBase, IColumnSource> ();
    private readonly OrderedDictionary _joinedTables = new OrderedDictionary();

    public JoinedTableContext (IDatabaseInfo databaseInfo)
    {
      ArgumentUtility.CheckNotNull ("databaseInfo", databaseInfo);
      _databaseInfo = databaseInfo;
    }

    public int Count
    {
      get { return _joinedTables.Count; }
    }

    public Table GetJoinedTable (IDatabaseInfo databaseInfo, FieldSourcePath fieldSourcePath, MemberInfo relationMember)
    {
      ArgumentUtility.CheckNotNull ("databaseInfo", databaseInfo);
      ArgumentUtility.CheckNotNull ("fieldSourcePath", fieldSourcePath);
      ArgumentUtility.CheckNotNull ("relationMember", relationMember);

      var key = new JoinedTableKey (fieldSourcePath, relationMember);
      
      if (!_joinedTables.Contains (key))
      {
        var table = databaseInfo.GetTableForRelation (relationMember, null);
        _joinedTables.Add (key, table);
      }

      return (Table) _joinedTables[key];
    }

    public void CreateAliases (QueryModel queryModel)
    {
      for (int i = 0; i < Count; ++i)
      {
        var table = (Table) _joinedTables[i];
        if (table.Alias == null)
          table.SetAlias (queryModel.GetNewName ("#j"));
      }
    }

    public IColumnSource GetColumnSource (FromClauseBase fromClause)
    {
      ArgumentUtility.CheckNotNull ("fromClause", fromClause);
      return GetOrCreateValue (_columnSources, fromClause, CreateColumnSource);
    }

    private IColumnSource GetOrCreateValue (Dictionary<FromClauseBase, IColumnSource> dictionary, FromClauseBase key, Func<FromClauseBase, IColumnSource> creator)
    {
      IColumnSource value;
      if (!dictionary.TryGetValue (key, out value))
      {
        value = creator (key);
        dictionary.Add (key, value);
      }
      return value;
    }

    private IColumnSource CreateColumnSource (FromClauseBase clause)
    {
      var subQueryExpression = clause.FromExpression as SubQueryExpression;
      if (subQueryExpression != null)
        return new SubQuery (subQueryExpression.QueryModel, ParseMode.SubQueryInFrom, clause.ItemName);
      else
        return _databaseInfo.GetTableForFromClause (clause, clause.ItemName);
    }
  }
}
