namespace AlibabaCloud.OSS.V2.UnitTests.Transform;

class ModelStub : V2.Models.RequestModel {

    public string HeaderParma {
        get => Headers.TryGetValue("header-param-1", out var value) ? value : null;
        set {
            if (value != null) Headers["header-param-1"] = value;
        }
    }

    public string QueryParma {
        get => Parameters.TryGetValue("query-param-1", out var value) ? value : null;
        set {
            if (value != null) Parameters["query-param-1"] = value;
        }
    }
}

public class FunctionsTest {
    [Fact]
    public void TestSerializeInputHeaderAndParameters() {
        // case 1
        var model = new ModelStub();
        var input = new OperationInput();
        V2.Transform.Serde.SerializeInput(model, ref input);
        Assert.Null(input.Headers);
        Assert.Null(input.Parameters);
        Assert.Null(input.Body);

        // case 2
        model = new ModelStub() {
            HeaderParma = "val-1",
            QueryParma  = "val-2"
        };
        input = new OperationInput();
        V2.Transform.Serde.SerializeInput(model, ref input);
        Assert.NotNull(input.Headers);
        Assert.Equal("val-1", input.Headers["header-param-1"]);
        Assert.NotNull(input.Parameters);
        Assert.Equal("val-2", input.Parameters["query-param-1"]);
        Assert.Null(input.Body);

        // case 3
        model = new ModelStub();
        input = new OperationInput() {
            Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
            Parameters = new Dictionary<string, string>(),
        };
        V2.Transform.Serde.SerializeInput(model, ref input);
        Assert.NotNull(input.Headers);
        Assert.Empty(input.Headers);
        Assert.NotNull(input.Parameters);
        Assert.Empty(input.Parameters);
        Assert.Null(input.Body);

        // case 4
        model = new ModelStub() {
            HeaderParma = "val-1",
            QueryParma  = "val-2"
        };
        input = new OperationInput() {
            Headers    = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
            Parameters = new Dictionary<string, string>(),
        };
        V2.Transform.Serde.SerializeInput(model, ref input);
        Assert.NotNull(input.Headers);
        Assert.Equal("val-1", input.Headers["header-param-1"]);
        Assert.NotNull(input.Parameters);
        Assert.Equal("val-2", input.Parameters["query-param-1"]);
        Assert.Null(input.Body);
    }

    [Fact]
    public void TestSerializeInputBody() {
        // InnerBody is null
        var model = new ModelStub();
        var input = new OperationInput();
        V2.Transform.Serde.SerializeInput(model, ref input);
        Assert.Null(input.Headers);
        Assert.Null(input.Parameters);
        Assert.Null(input.Body);

        // InnerBody is not null and BodyFormat is xml

        // InnerBody is not null and InnerBody is Stream
        model = new ModelStub() {
            InnerBody  = new MemoryStream(),
            BodyFormat = "",
        };
        input = new OperationInput();
        V2.Transform.Serde.SerializeInput(model, ref input);
        Assert.Null(input.Headers);
        Assert.Null(input.Parameters);
        Assert.NotNull(input.Body);
        Assert.IsAssignableFrom<MemoryStream>(input.Body);

        // InnerBody is not null and InnerBody is not supported type
        model = new ModelStub() {
            InnerBody  = "hello world",
            BodyFormat = "",
        };
        input = new OperationInput();

        try {
            V2.Transform.Serde.SerializeInput(model, ref input);
            Assert.Fail("should not here");
        }
        catch (Exception e) {
            Assert.Contains("not support body type", e.ToString());
        }
    }

    [Fact]
    public void TestToBool() {
        Assert.True(Convert.ToBoolean("true"));
        Assert.True(Convert.ToBoolean("True"));
        Assert.False(Convert.ToBoolean("false"));
        Assert.False(Convert.ToBoolean("False"));

        Assert.False(Convert.ToBoolean(null));
    }

    [Fact]
    public void TestEscape() {
        Assert.Null(V2.Transform.Serde.EscapeXml(null));
        Assert.Equal("", V2.Transform.Serde.EscapeXml(""));
        Assert.Equal("hello world", V2.Transform.Serde.EscapeXml("hello world"));
        Assert.Equal("&lt;&gt;&amp;&quot;&apos;", V2.Transform.Serde.EscapeXml("<>&\"'"));
        Assert.Equal("hello&lt;&gt;&amp;&quot;&apos;world", V2.Transform.Serde.EscapeXml("hello<>&\"'world"));

        var data = new byte[]
            { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f };
        var encStr = "&#00;&#01;&#02;&#03;&#04;&#05;&#06;&#07;&#08;&#09;&#10;&#11;&#12;&#13;&#14;&#15;";
        var oriStr = System.Text.Encoding.UTF8.GetString(data);
        Assert.Equal(encStr, V2.Transform.Serde.EscapeXml(Convert.ToString(oriStr)));

        data = [
            0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18, 0x19, 0x1a, 0x1b, 0x1c, 0x1d, 0x1e, 0x1f, 0x20, 0x21,
            0xe4, 0xbd, 0xa0, 0xe5, 0xa5, 0xbd
        ];
        encStr = "&#16;&#17;&#18;&#19;&#20;&#21;&#22;&#23;&#24;&#25;&#26;&#27;&#28;&#29;&#30;&#31; !你好";
        oriStr = System.Text.Encoding.UTF8.GetString(data);
        Assert.Equal(encStr, V2.Transform.Serde.EscapeXml(Convert.ToString(oriStr)));
    }
}