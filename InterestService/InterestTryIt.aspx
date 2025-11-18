
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="InterestTryIt.aspx.cs" Inherits="InterestService.InterestTryIt" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
                <meta charset="utf-8" />
    <title>InterestService TryIt</title>
</head>
<body>
    <form id="form1" runat="server">
        <div style="max-width: 400px; margin: 40px 0; font-family: 'Times New Roman', Times, serif; text-align:left;">
            <h2>InterestService TryIt</h2>

            <p>
                Description: Compound Interest Calculator</p>
            <p>
                Service URL: <a href="http://webstrar115.fulton.asu.edu/Page2/InterestTryIt.aspx">http://webstrar115.fulton.asu.edu/Page2/InterestTryIt.aspx</a></p>
            <p>
                Method: CalculateCompoundInterest(double balance, double annualIntRatePercent, int years, int compoundsPerYear)</p>
            <p>
                <asp:Label runat="server" AssociatedControlID="txtBalance" Text="Balance (principal):" />
                <asp:TextBox runat="server" ID="txtBalance" />
            </p>

            <p>
                <asp:Label runat="server" AssociatedControlID="txtRate" Text="Annual rate (%):" />
                <asp:TextBox runat="server" ID="txtRate" />
            </p>

            <p>
                <asp:Label runat="server" AssociatedControlID="txtYears" Text="Years:" />
                <asp:TextBox runat="server" ID="txtYears" />
            </p>

            <p>
                <asp:Label runat="server" AssociatedControlID="txtCompounds" Text="Compounds per year:" />
                <asp:TextBox runat="server" ID="txtCompounds" Text="12" />
            </p>

            <p>
                <asp:Button runat="server" ID="btnCalculate" Text="Calculate"
                            OnClick="btnCalculate_Click" />
            </p>

            <p>
                <asp:Label runat="server" ID="lblResult" />
            </p>
        </div>
    </form>
</body>
</html>
