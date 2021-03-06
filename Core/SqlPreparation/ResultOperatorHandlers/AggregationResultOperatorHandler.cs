// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (c) rubicon IT GmbH, www.rubicon.eu
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
using Remotion.Linq.Clauses;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Utilities;

namespace Remotion.Linq.SqlBackend.SqlPreparation.ResultOperatorHandlers
{
  /// <summary>
  /// <see cref="AggregationResultOperatorHandler{T}"/> is the base class for all specific aggregation <see cref="ResultOperatorHandler{T}"/>s.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public abstract class AggregationResultOperatorHandler<T> : ResultOperatorHandler<T>
      where T: ResultOperatorBase
  {
    public abstract AggregationModifier AggregationModifier { get; }

    public override void HandleResultOperator (
        T resultOperator,
        SqlStatementBuilder sqlStatementBuilder,
        UniqueIdentifierGenerator generator,
        ISqlPreparationStage stage,
        ISqlPreparationContext context)
    {
      ArgumentUtility.CheckNotNull ("resultOperator", resultOperator);
      ArgumentUtility.CheckNotNull ("sqlStatementBuilder", sqlStatementBuilder);
      ArgumentUtility.CheckNotNull ("generator", generator);
      ArgumentUtility.CheckNotNull ("stage", stage);
      ArgumentUtility.CheckNotNull ("context", context);

      EnsureNoTopExpression (sqlStatementBuilder, generator, stage, context);
      EnsureNoGroupExpression (sqlStatementBuilder, generator, stage, context);
      EnsureNoDistinctQuery (sqlStatementBuilder, generator, stage, context);
      EnsureNoSetOperations (sqlStatementBuilder, generator, stage, context);
      UpdateDataInfo (resultOperator, sqlStatementBuilder, sqlStatementBuilder.DataInfo);

      sqlStatementBuilder.Orderings.Clear ();

      sqlStatementBuilder.SelectProjection = new AggregationExpression (
          sqlStatementBuilder.DataInfo.DataType, 
          sqlStatementBuilder.SelectProjection, 
          AggregationModifier);
    }
  }
}