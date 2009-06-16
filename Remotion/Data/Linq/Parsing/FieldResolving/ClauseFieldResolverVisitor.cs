// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// version 3.0 as published by the Free Software Foundation.
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
using System.Linq.Expressions;
using System.Reflection;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Parsing.FieldResolving
{
  /// <summary>
  /// Identifies the query source and members used by a field access expression.
  /// </summary>
  public class ClauseFieldResolverVisitor : ExpressionTreeVisitor
  {
    public static FieldAccessInfo ParseFieldAccess (
        IDatabaseInfo databaseInfo,
        IResolveableClause resolvedClause,
        Expression fieldAccessExpression,
        bool optimizeRelatedKeyAccess)
    {
      ArgumentUtility.CheckNotNull ("databaseInfo", databaseInfo);
      ArgumentUtility.CheckNotNull ("resolvedClause", resolvedClause);
      ArgumentUtility.CheckNotNull ("fieldAccessExpression", fieldAccessExpression);

      var visitor = new ClauseFieldResolverVisitor (databaseInfo, resolvedClause, optimizeRelatedKeyAccess);
      visitor.VisitExpression (fieldAccessExpression);
      return new FieldAccessInfo (visitor._accessedMember, visitor._joinMembers.ToArray ());
    }

    private readonly IDatabaseInfo _databaseInfo;
    private readonly IResolveableClause _resolvedClause;
    private readonly List<MemberInfo> _joinMembers;
    private readonly bool _optimizeRelatedKeyAccess;

    private MemberInfo _accessedMember;

    private ClauseFieldResolverVisitor (IDatabaseInfo databaseInfo, IResolveableClause resolvedClause, bool optimizeRelatedKeyAccess)
    {
      ArgumentUtility.CheckNotNull ("databaseInfo", databaseInfo);
      ArgumentUtility.CheckNotNull ("resolvedClause", resolvedClause);

      _databaseInfo = databaseInfo;
      _optimizeRelatedKeyAccess = optimizeRelatedKeyAccess;
      _resolvedClause = resolvedClause;
      _joinMembers = new List<MemberInfo> ();
      _accessedMember = null;
    }

    protected override Expression VisitExpression (Expression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      if (expression is ParameterExpression || expression is MemberExpression || expression is QuerySourceReferenceExpression)
        return base.VisitExpression (expression);
      else
      {
        string message = string.Format ("Only MemberExpressions, QuerySourceReferenceExpressions, and ParameterExpressions can be resolved, found "
            + "'{0}'.", expression);
        throw new FieldAccessResolveException (message);
      }
    }

    protected override Expression VisitParameterExpression (ParameterExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      if (expression.Name != _resolvedClause.Identifier.Name)
      {
        string message = string.Format ("This clause can only resolve field accesses for parameters called '{0}', but a parameter "
                                        + "called '{1}' was given.", _resolvedClause.Identifier.Name, expression.Name);
        throw new FieldAccessResolveException (message);
      }

      if (expression.Type != _resolvedClause.Identifier.Type)
      {
        string message = string.Format ("This clause can only resolve field accesses for parameters of type '{0}', but a parameter "
                                        + "of type '{1}' was given.", _resolvedClause.Identifier.Type, expression.Type);
        throw new FieldAccessResolveException (message);
      }

      return base.VisitParameterExpression (expression);
    }

    protected override Expression VisitQuerySourceReferenceExpression (QuerySourceReferenceExpression expression)
    {
      if (expression.ReferencedClause != _resolvedClause)
      {
        string message = string.Format ("This clause can only resolve field accesses for itself ('{0}'), but a reference to a clause "
                                        + "called '{1}' was given.", _resolvedClause.Identifier.Name, expression.ReferencedClause.Identifier.Name);
        throw new FieldAccessResolveException (message);
      }

      return base.VisitQuerySourceReferenceExpression (expression);
    }

    protected override Expression VisitMemberExpression (MemberExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      bool isFirstMember = _accessedMember == null;
      if (isFirstMember && (!_optimizeRelatedKeyAccess || !IsOptimizableRelatedKeyAccess(expression)))
      {
        // for non-optimized (or non-optimizable) related key access, we leave _accessedMember null, we'll take the next one
        // eg. sd.Student.ID => we'll take sd.Student, not ID as the accessed member
        _accessedMember = expression.Member;
      }

      Expression result = base.VisitMemberExpression (expression);

      if (!isFirstMember)
        _joinMembers.Add (expression.Member);
      return result;
    }

    private bool IsOptimizableRelatedKeyAccess (MemberExpression expression)
    {
      var primaryKeyMember = _databaseInfo.GetPrimaryKeyMember (expression.Expression.Type);
      return expression.Member.Equals (primaryKeyMember);
    }
  }
}
