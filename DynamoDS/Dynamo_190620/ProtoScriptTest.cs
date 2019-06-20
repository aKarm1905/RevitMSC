using NUnit.Framework;
namespace ProtoTest
{
    [TestFixture]
    class ProtoScriptTest : ProtoTestBase
    {
        [Test]
        public void BasicInfrastructureTest()
        {
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            runtimeCore = fsr.Execute(
@"
        }
    }

    [TestFixture]
    class MultiLangNegitiveTests : ProtoTestBase
    {
        //Negitive Tests with distortions of the Language def block
        [Test]
        public void ParserFailTest1()
        {
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            //@TODO: What exception should this throw
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                runtimeCore = fsr.Execute(
    @"
            });
        }
        [Test]
        public void ParserFailTest2()
        {
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            //@TODO: What exception should this throw
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                runtimeCore = fsr.Execute(
    @"
            });
        }
        [Test]
        public void ParserFailTest3()
        {
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            //@TODO: What exception should this throw
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                runtimeCore = fsr.Execute(
    @"
            });
        }
    }
}