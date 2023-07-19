using System;
using Faqt;
using Xunit;

internal class TestUtils
{
  internal static void AssertExnMsg(Action f, string msg)
  {
    var ex = Assert.Throws<AssertionFailedException>(f);
    Assert.Equal(
      msg.ReplaceLineEndings("\n").Trim(),
      ex.Message.ReplaceLineEndings("\n").Trim().Replace("\t", "    "));
  }
}
