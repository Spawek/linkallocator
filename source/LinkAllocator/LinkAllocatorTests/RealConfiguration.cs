using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LinkAllocator;

namespace LinkAllocatorTests
{
    [TestClass]
    public class RealConfiguration
    {
        [TestMethod]
        public void L111_A_5MHz_FSME_FXEA_Configuration()
        {
            Topology t = SyslogParser.CreateTopology("../../../../Configurations/L111_A_5MHz_FSME_FXEA_Configuration.log");

            t.AllocateLinksPaths();
            t.AllocateSlots();
        }

        [TestMethod]
        public void T3_20MHz_8TXRX_FSIH_FZHJ_Configuration()
        {
            Topology t = SyslogParser.CreateTopology("../../../../Configurations/T3_20MHz_8TXRX_FSIH_FZHJ_Configuration.log");

            t.AllocateLinksPaths();
            t.AllocateSlots();
        }

        [TestMethod]
        public void L222_4TX_4RX_20MHz_FSMF_2FBBA_3FZNI_Configuration()
        {
            Topology t = SyslogParser.CreateTopology("../../../../Configurations/L222_4TX_4RX_20MHz_FSMF_2FBBA_3FZNI_Configuration.log");

            t.AllocateLinksPaths();
            t.AllocateSlots();
        }
    }
}
