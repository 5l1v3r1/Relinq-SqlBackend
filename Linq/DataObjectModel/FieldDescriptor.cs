using System;
using System.Reflection;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Parsing.FieldResolving;
using Remotion.Utilities;

namespace Remotion.Data.Linq.DataObjectModel
{
  public struct FieldDescriptor
  {
    public FieldDescriptor (MemberInfo member, FieldSourcePath sourcePath, Column? column)
        : this()
    {
      ArgumentUtility.CheckNotNull ("sourcePath", sourcePath);

      if (member == null && column == null)
        throw new ArgumentNullException ("member && column", "Either member or column must have a value.");

      Member = member;
      Column = column;
      SourcePath = sourcePath;
    }

    public MemberInfo Member { get; private set; }
    public Column? Column { get; private set; }
    public FieldSourcePath SourcePath { get; private set; }


    public Column GetMandatoryColumn ()
    {
      if (Column != null)
        return Column.Value;
      else
      {
        string message = string.Format ("The member '{0}.{1}' does not identify a queryable column.",
            Member.DeclaringType.FullName, Member.Name);

        throw new FieldAccessResolveException (message);
      }
    }
    
    public override string ToString ()
    {
      return string.Format ("{0} => {1}", SourcePath, Column);
    }
  }
}