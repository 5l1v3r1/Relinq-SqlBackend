// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// re-linq is free software; you can redistribute it and/or modify it under 
// the terms of the GNU Lesser General Public License as published by the 
// Free Software Foundation; either version 2.1 of the License, 
// or (at your option) any later version.
// 
// re-linq is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-linq; if not, see http://www.gnu.org/licenses.
// 
using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ExpressionTreeVisitors;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Linq.Utilities;

namespace Remotion.Linq.SqlBackend.SqlPreparation
{
  /// <summary>
  /// Analyzes the <see cref="FromClauseBase.FromExpression"/> of a <see cref="FromClauseBase"/> and returns a <see cref="SqlTableBase"/> that 
  /// represents the data source of the <see cref="FromClauseBase"/>.
  /// </summary>
  public class SqlPreparationFromExpressionVisitor : SqlPreparationExpressionVisitor, IUnresolvedSqlExpressionVisitor
  {
    public static FromExpressionInfo AnalyzeFromExpression (
        Expression fromExpression,
        ISqlPreparationStage stage,
        UniqueIdentifierGenerator generator,
        IMethodCallTransformerProvider provider,
        ISqlPreparationContext context,
        Func<ITableInfo, SqlTableBase> tableGenerator)
    {
      ArgumentUtility.CheckNotNull ("fromExpression", fromExpression);
      ArgumentUtility.CheckNotNull ("stage", stage);
      ArgumentUtility.CheckNotNull ("generator", generator);
      ArgumentUtility.CheckNotNull ("provider", provider);
      ArgumentUtility.CheckNotNull ("context", context);

      var visitor = new SqlPreparationFromExpressionVisitor (generator, stage, provider, context, tableGenerator);
      visitor.VisitExpression (fromExpression);
      if (visitor._fromExpressionInfo != null)
        return visitor._fromExpressionInfo.Value;

      var message = string.Format (
          "Error parsing expression '{0}'. Expressions of type '{1}' cannot be used as the SqlTables of a from clause.",
          FormattingExpressionTreeVisitor.Format (fromExpression),
          fromExpression.Type.Name);
      throw new NotSupportedException (message);
    }

    private readonly UniqueIdentifierGenerator _generator;
    private readonly ISqlPreparationContext _context;

    private FromExpressionInfo? _fromExpressionInfo;
    private readonly Func<ITableInfo, SqlTableBase> _tableGenerator;

    protected FromExpressionInfo? FromExpressionInfo
    {
      get { return _fromExpressionInfo; }
    }

    protected SqlPreparationFromExpressionVisitor (
        UniqueIdentifierGenerator generator,
        ISqlPreparationStage stage,
        IMethodCallTransformerProvider provider,
        ISqlPreparationContext context,
        Func<ITableInfo, SqlTableBase> tableGenerator)
        : base (context, stage, provider)
    {
      _generator = generator;
      _context = context;
      _fromExpressionInfo = null;
      _tableGenerator = tableGenerator;
    }

    protected override Expression VisitConstantExpression (ConstantExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      var itemType = ReflectionUtility.GetItemTypeOfIEnumerable (expression.Type, "from expression");
      var sqlTable = _tableGenerator (new UnresolvedTableInfo (itemType));
      var sqlTableReferenceExpression = new SqlTableReferenceExpression (sqlTable);
      _fromExpressionInfo = new FromExpressionInfo (sqlTable, new Ordering[0], sqlTableReferenceExpression, null);

      return sqlTableReferenceExpression;
    }

    protected override Expression VisitMemberExpression (MemberExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      var preparedMemberExpression = (MemberExpression) TranslateExpression (expression, _context, Stage, MethodCallTransformerProvider);

      var joinInfo = new UnresolvedCollectionJoinInfo (preparedMemberExpression.Expression, preparedMemberExpression.Member);
      var joinedTable = new SqlJoinedTable (joinInfo, JoinSemantics.Inner);
      var oldStyleJoinedTable = _tableGenerator (joinedTable);
      var sqlTableReferenceExpression = new SqlTableReferenceExpression (oldStyleJoinedTable);
      _fromExpressionInfo = new FromExpressionInfo (
          oldStyleJoinedTable, new Ordering[0], sqlTableReferenceExpression, new JoinConditionExpression (joinedTable));

      return sqlTableReferenceExpression;
    }

    public override Expression VisitSqlSubStatementExpression (SqlSubStatementExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      var sqlStatement = expression.SqlStatement;

      var factory = new SqlPreparationSubStatementTableFactory (Stage, _context, _generator);
      _fromExpressionInfo = factory.CreateSqlTableForStatement (sqlStatement, _tableGenerator);
      Debug.Assert (_fromExpressionInfo.Value.WhereCondition == null);

      return new SqlTableReferenceExpression (_fromExpressionInfo.Value.SqlTable);
    }

    public Expression VisitSqlTableReferenceExpression (SqlTableReferenceExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      var tableInfo = new UnresolvedGroupReferenceTableInfo (expression.SqlTable);
      var sqlTable = new SqlTable (tableInfo, JoinSemantics.Inner);
      _fromExpressionInfo = new FromExpressionInfo (sqlTable, new Ordering[0], new SqlTableReferenceExpression (sqlTable), null);

      return expression;
    }

    protected override Expression VisitQuerySourceReferenceExpression (QuerySourceReferenceExpression expression)
    {
      var groupJoinClause = expression.ReferencedQuerySource as GroupJoinClause;
      if (groupJoinClause != null)
      {
        var fromExpressionInfo = AnalyzeFromExpression (
            groupJoinClause.JoinClause.InnerSequence,
            Stage,
            _generator,
            MethodCallTransformerProvider,
            _context,
            _tableGenerator);

        _context.AddExpressionMapping (new QuerySourceReferenceExpression (groupJoinClause.JoinClause), fromExpressionInfo.ItemSelector);

        var whereCondition =
            Stage.PrepareWhereExpression (
                Expression.Equal (groupJoinClause.JoinClause.OuterKeySelector, groupJoinClause.JoinClause.InnerKeySelector), _context);

        if (fromExpressionInfo.WhereCondition != null)
          whereCondition = Expression.AndAlso (fromExpressionInfo.WhereCondition, whereCondition);

        _fromExpressionInfo = new FromExpressionInfo (
            fromExpressionInfo.SqlTable,
            fromExpressionInfo.ExtractedOrderings.ToArray(),
            fromExpressionInfo.ItemSelector,
            whereCondition);

        return new SqlTableReferenceExpression (fromExpressionInfo.SqlTable);
      }

      return base.VisitQuerySourceReferenceExpression (expression);
    }

    Expression IUnresolvedSqlExpressionVisitor.VisitSqlEntityRefMemberExpression (SqlEntityRefMemberExpression expression)
    {
      return VisitExtensionExpression (expression);
    }

    Expression IUnresolvedSqlExpressionVisitor.VisitSqlEntityConstantExpression (SqlEntityConstantExpression expression)
    {
      return VisitExtensionExpression (expression);
    }
  }
}