using System;
using System.Reflection;
using NUnit.Framework;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Parsing.FieldResolving;

namespace Remotion.Data.Linq.UnitTests.DataObjectModelTest
{
  [TestFixture]
  public class FieldDescriptorTest
  {
    [Test]
    [ExpectedException (typeof (ArgumentNullException),ExpectedMessage = "Either member or column must have a value.\r\n"+
      "Parameter name: member && column")]
    public void MemberAndColumnNull()
    {
      new FieldDescriptor (null, ExpressionHelper.GetPathForNewTable(), null);
    }

    [Test]
    public void MemberNull()
    {
      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause();
      Column column = new Column();
      FieldSourcePath path = ExpressionHelper.GetPathForNewTable();
      FieldDescriptor descriptor = new FieldDescriptor (null, path, column);
      Assert.IsNull (descriptor.Member);
      //Assert.AreSame (fromClause, descriptor.FromClause);
      Assert.AreEqual (column, descriptor.Column);
      Assert.AreEqual (path, descriptor.SourcePath);
    }

    [Test]
    public void ColumnNull ()
    {
      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause ();
      FieldSourcePath path = ExpressionHelper.GetPathForNewTable();
      MemberInfo member = typeof (Student).GetProperty ("First");
      FieldDescriptor descriptor = new FieldDescriptor (member,path, null);
      Assert.IsNull (descriptor.Column);
      //Assert.AreSame (fromClause, descriptor.FromClause);
      Assert.AreEqual (member, descriptor.Member);
      Assert.AreEqual (path, descriptor.SourcePath);
    }

    [Test]
    public void MemberAndColumnNotNull ()
    {
      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause ();
      FieldSourcePath path = ExpressionHelper.GetPathForNewTable ();
      MemberInfo member = typeof (Student).GetProperty ("First");
      Column column = new Column ();
      FieldDescriptor descriptor = new FieldDescriptor (member, path, column);
      Assert.AreEqual (column, descriptor.Column);
      //Assert.AreSame (fromClause, descriptor.FromClause);
      Assert.AreEqual (member, descriptor.Member);
      Assert.AreEqual (path, descriptor.SourcePath);
    }
    
    [Test]
    public void GetMandatoryColumn()
    {
      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause ();
      FieldSourcePath path = ExpressionHelper.GetPathForNewTable ();
      MemberInfo member = typeof (Student).GetProperty ("First");
      Column column = new Column ();
      FieldDescriptor descriptor = new FieldDescriptor (member, path, column);
      Assert.AreEqual (column, descriptor.GetMandatoryColumn());
    }
    
    [Test]
    [ExpectedException (typeof (FieldAccessResolveException), ExpectedMessage = "The member 'Remotion.Data.Linq.UnitTests.Student.First' "
      + "does not identify a queryable column.")]
    public void GetMandatoryColumnWithException ()
    {
      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause ();
      FieldSourcePath path = ExpressionHelper.GetPathForNewTable ();
      MemberInfo member = typeof (Student).GetProperty ("First");
      new FieldDescriptor (member, path, null).GetMandatoryColumn ();
    }
    
  }
}