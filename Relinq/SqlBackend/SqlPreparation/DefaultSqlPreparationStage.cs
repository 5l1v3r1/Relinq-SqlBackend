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
using System.Linq.Expressions;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.Utilities;

namespace Remotion.Linq.SqlBackend.SqlPreparation
{
  /// <summary>
  /// Provides a default implementation of <see cref="ISqlPreparationStage"/>.
  /// </summary>
  public class DefaultSqlPreparationStage : ISqlPreparationStage
  {
    private readonly IMethodCallTransformerProvider _methodCallTransformerProvider;
    private readonly UniqueIdentifierGenerator _generator;
    private readonly ResultOperatorHandlerRegistry _resultOperatorHandlerRegistry;

    public DefaultSqlPreparationStage (
        IMethodCallTransformerProvider methodCallTransformerProvider,
        ResultOperatorHandlerRegistry resultOperatorHandlerRegistry,
        UniqueIdentifierGenerator generator)
    {
      ArgumentUtility.CheckNotNull ("methodCallTransformerProvider", methodCallTransformerProvider);
      ArgumentUtility.CheckNotNull ("resultOperatorHandlerRegistry", resultOperatorHandlerRegistry);
      ArgumentUtility.CheckNotNull ("generator", generator);

      _methodCallTransformerProvider = methodCallTransformerProvider;
      _resultOperatorHandlerRegistry = resultOperatorHandlerRegistry;
      _generator = generator;
    }

    public IMethodCallTransformerProvider MethodCallTransformerProvider
    {
      get { return _methodCallTransformerProvider; }
    }

    public ResultOperatorHandlerRegistry ResultOperatorHandlerRegistry
    {
      get { return _resultOperatorHandlerRegistry; }
    }

    public virtual Expression PrepareSelectExpression (Expression expression, ISqlPreparationContext context)
    {
      return PrepareExpression (expression, context);
    }

    public virtual Expression PrepareWhereExpression (Expression expression, ISqlPreparationContext context)
    {
      return PrepareExpression (expression, context);
    }

    public virtual Expression PrepareTopExpression (Expression expression, ISqlPreparationContext context)
    {
      return PrepareExpression (expression, context);
    }

    public virtual Expression PrepareOrderByExpression (Expression expression, ISqlPreparationContext context)
    {
      return PrepareExpression (expression, context);
    }

    public virtual Expression PrepareResultOperatorItemExpression (Expression expression, ISqlPreparationContext context)
    {
      return PrepareExpression (expression, context);
    }

    public virtual FromExpressionInfo PrepareFromExpression (
        Expression fromExpression,
        ISqlPreparationContext context,
        Func<ITableInfo, SqlTableBase> tableGenerator)
    {
      return SqlPreparationFromExpressionVisitor.AnalyzeFromExpression (
          fromExpression, this, _generator, _methodCallTransformerProvider, context, tableGenerator);
    }

    public virtual SqlStatement PrepareSqlStatement (QueryModel queryModel, ISqlPreparationContext parentContext)
    {
      return SqlPreparationQueryModelVisitor.TransformQueryModel (queryModel, parentContext, this, _generator, _resultOperatorHandlerRegistry);
    }

    protected virtual Expression PrepareExpression (Expression expression, ISqlPreparationContext context)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      return SqlPreparationExpressionVisitor.TranslateExpression (expression, context, this, _methodCallTransformerProvider);
    }
  }
}