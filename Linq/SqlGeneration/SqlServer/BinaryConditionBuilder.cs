using System;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq.SqlGeneration.SqlServer
{
  public class BinaryConditionBuilder
  {
    private readonly CommandBuilder _commandBuilder;

    public BinaryConditionBuilder (CommandBuilder commandBuilder)
    {
      ArgumentUtility.CheckNotNull ("command", commandBuilder);
      _commandBuilder = commandBuilder;
    }

    public void BuildBinaryConditionPart (BinaryCondition binaryCondition)
    {
      if (binaryCondition.Left.Equals (new Constant (null)))
        AppendNullCondition (binaryCondition.Right, binaryCondition.Kind);
      else if (binaryCondition.Right.Equals (new Constant (null)))
        AppendNullCondition (binaryCondition.Left, binaryCondition.Kind);
      else
      {
        _commandBuilder.Append ("(");
        AppendNullChecksForBinaryConditions (binaryCondition.Left, binaryCondition.Right, binaryCondition.Kind);

        AppendValueInCondition (binaryCondition.Left);
        _commandBuilder.Append (" ");
        AppendBinaryConditionKind (binaryCondition.Kind);
        _commandBuilder.Append (" ");
        AppendValueInCondition (binaryCondition.Right);
        _commandBuilder.Append (")");
      }
    }

    private void AppendNullCondition (IValue value, BinaryCondition.ConditionKind kind)
    {
      AppendValueInCondition (value);
      switch (kind)
      {
        case BinaryCondition.ConditionKind.Equal:
          _commandBuilder.Append (" IS NULL");
          break;
        default:
          Assertion.IsTrue (kind == BinaryCondition.ConditionKind.NotEqual, "null can only be compared via == and !=");
          _commandBuilder.Append (" IS NOT NULL");
          break;
      }
    }

    private void AppendNullChecksForBinaryConditions (IValue left, IValue right, BinaryCondition.ConditionKind conditionKind)
    {
      if (left is Column || right is Column)
      {
        switch (conditionKind)
        {
          case BinaryCondition.ConditionKind.Equal:
          case BinaryCondition.ConditionKind.LessThanOrEqual:
          case BinaryCondition.ConditionKind.GreaterThanOrEqual:
            if (left is Column && right is Column)
            {
              _commandBuilder.Append ("(");
              AppendNullCondition (left, BinaryCondition.ConditionKind.Equal);
              _commandBuilder.Append (" AND ");
              AppendNullCondition (right, BinaryCondition.ConditionKind.Equal);
              _commandBuilder.Append (") OR ");
            }
            break;
          case BinaryCondition.ConditionKind.NotEqual:
            if (left is Column && right is Column)
            {
              _commandBuilder.Append ("(");
              AppendNullCondition (left, BinaryCondition.ConditionKind.Equal);
              _commandBuilder.Append (" AND ");
              AppendNullCondition (right, BinaryCondition.ConditionKind.NotEqual);
              _commandBuilder.Append (") OR ");
              _commandBuilder.Append ("(");
              AppendNullCondition (left, BinaryCondition.ConditionKind.NotEqual);
              _commandBuilder.Append (" AND ");
              AppendNullCondition (right, BinaryCondition.ConditionKind.Equal);
              _commandBuilder.Append (") OR ");
            }
            else if (left is Column)
            {
              AppendNullCondition (left, BinaryCondition.ConditionKind.Equal);
              _commandBuilder.Append (" OR ");
            }
            else
            {
              AppendNullCondition (right, BinaryCondition.ConditionKind.Equal);
              _commandBuilder.Append (" OR ");
            }
            break;
        }
      }
    }

    private void AppendValueInCondition (IValue value)
    {
      if (value is Constant)
        _commandBuilder.AppendConstant ((Constant) value);
      else if (value is Column)
        _commandBuilder.AppendColumn ((Column) value);
      else
        throw new NotSupportedException ("Value type " + value.GetType ().Name + " is not supported.");
    }

    private void AppendBinaryConditionKind (BinaryCondition.ConditionKind kind)
    {
      string commandString;
      switch (kind)
      {
        case BinaryCondition.ConditionKind.Equal:
          commandString = "=";
          break;
        case BinaryCondition.ConditionKind.NotEqual:
          commandString = "<>";
          break;
        case BinaryCondition.ConditionKind.LessThan:
          commandString = "<";
          break;
        case BinaryCondition.ConditionKind.LessThanOrEqual:
          commandString = "<=";
          break;
        case BinaryCondition.ConditionKind.GreaterThan:
          commandString = ">";
          break;
        case BinaryCondition.ConditionKind.GreaterThanOrEqual:
          commandString = ">=";
          break;
        case BinaryCondition.ConditionKind.Like:
          commandString = "LIKE";
          break;
        default:
          throw new NotSupportedException ("The binary condition kind " + kind + " is not supported.");
      }
      _commandBuilder.Append (commandString);
    }
  }
}