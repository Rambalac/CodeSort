using Xunit;
using CodeSort;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeSort.Tests
{
    public class CodeSortTests
    {
        [Fact]
        public void SortFileTest()
        {
            var obj = new ClassMemberSorter();
            obj.SortAllFiles(@"D:\dev2\AmazonCloudDriveApi");
            //obj.SortFile(@"D:\dev2\AmazonCloudDriveApi\AmazonNodes.cs");
        }
    }
}