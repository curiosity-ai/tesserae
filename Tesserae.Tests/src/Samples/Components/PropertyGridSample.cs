using System;
using static Transpose.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 209, Icon = UIcons.ListCheck)]
    public class PropertyGridSample : IComponent, ISample
    {
        public enum AccountTier { Free, Pro, Enterprise }

        public class Address
        {
            public string Street  { get; set; }
            public string City    { get; set; }
            public string ZipCode { get; set; }
        }

        public class UserProfile
        {
            public string      Name          { get; set; }
            public string      Email         { get; set; }
            public string      Bio           { get; set; }
            public int         Age           { get; set; }
            public double      CreditBalance { get; set; }
            public bool        IsActive      { get; set; }
            public AccountTier Tier          { get; set; }
            public DateTime    MemberSince   { get; set; }
            public Color       FavoriteColor { get; set; }
            public Address     HomeAddress   { get; set; }
            public string      AccountId     { get; set; }
        }

        private readonly IComponent _content;

        public PropertyGridSample()
        {
            var profile = new UserProfile
            {
                Name          = "Ada Lovelace",
                Email         = "ada@example.com",
                Bio           = "Mathematician and writer, known for work on Charles Babbage's Analytical Engine.",
                Age           = 36,
                CreditBalance = 128.50,
                IsActive      = true,
                Tier          = AccountTier.Pro,
                MemberSince   = new DateTime(2021, 4, 12, 9, 30, 0),
                FavoriteColor = Color.FromArgb(0x33, 0x99, 0xFF),
                HomeAddress   = new Address { Street = "12 St James's Square", City = "London", ZipCode = "SW1Y 4LE" },
                AccountId     = "usr_8f3a91c2"
            };

            var readout  = TextBlock("").Secondary();
            var validator = Validator();

            var grid = PropertyGrid(profile)
               .WithValidator(validator)
               .Label("Bio", "Biography")
               .Multiline("Bio")
               .Description("Email", "Used for sign-in and notifications.")
               .Description("CreditBalance", "Account balance in USD.")
               .Order("Name", 1)
               .Order("Email", 2)
               .Order("Bio", 3)
               .ReadOnly("AccountId")
               .Validate("Name", v => string.IsNullOrWhiteSpace(v as string) ? "Name is required" : null)
               .Validate("Email", v =>
               {
                   var email = v as string ?? "";
                   return email.Contains("@") && email.Contains(".") ? null : "Enter a valid email address";
               })
               .OnChange(p => readout.Text = $"{p.Name} · {p.Email} · age {p.Age} · {p.Tier} · active: {p.IsActive} · balance: {p.CreditBalance}");

            readout.Text = $"{profile.Name} · {profile.Email} · age {profile.Age} · {profile.Tier} · active: {profile.IsActive} · balance: {profile.CreditBalance}";

            _content = SectionStack().Secondary()
               .SampleTitle(typeof(PropertyGridSample), UIcons.ListCheck, "Metadata-driven, two-way-bound property editor")
               .Section(Stack().Children(
                    Card(VStack().WS().Children(
                        TextBlock("PropertyGrid reflects over a typed object and auto-generates an editing form, mapping each property type to an existing Tesserae input (string→TextBox, multiline→TextArea, numbers→NumberPicker, bool→Toggle, enum→Dropdown, DateTime→DateTimePicker, Color→ColorPicker, and nested objects→a recursive grouped Expander). Edits flow straight back onto the object and are surfaced via an observable. Labels, descriptions, ordering, read-only and validation are configurable."))).SetTitle("Overview")))
               .Section(Stack().Children(
                    Card(VStack().WS().Children(
                        grid.WS(),
                        HStack().WS().PT(12).Children(
                            Button("Validate").Primary().SetIcon(UIcons.CheckCircle).OnClick(() =>
                            {
                                if (validator.IsValid) Toast().Success("Valid", "All fields are valid");
                                else Toast().Warning("Invalid", "Please fix the highlighted fields");
                            })))).SetTitle("Edit profile")))
               .Section(Stack().Children(
                    Card(VStack().WS().Children(
                        SampleSubTitle("Live bound value"),
                        readout)).SetTitle("Two-way binding")))
               .Section(Stack().Children(
                    Card(VStack().WS().Children(
                        TextBlock("Calling .ReadOnly() with no arguments renders the whole grid as a non-editable view of the same object — every field shows its value but cannot be changed. Passing property names instead restricts read-only to just those fields."),
                        PropertyGrid(profile).ReadOnly().Ignore("AccountId").WS())).SetTitle("Read-only view")));
        }

        public HTMLElement Render() => _content.Render();
    }
}
