1. **Define `OmniBoxOptions` and Enum**
   - Create `OmniBoxMode` enum (`Search`, `Chat`, `SearchAndChat`) in `Tesserae/src/Components/OmniBox.cs`.
   - Create `OmniBoxOptions` class to encapsulate `Mode`, `History`, `Multiline` in `Tesserae/src/Components/OmniBox.cs`.
2. **Update `OmniBox` Constructor and UI Components**
   - Modify the constructor of `OmniBox` in `Tesserae/src/Components/OmniBox.cs` to accept `OmniBoxOptions` and a `placeholder`.
   - Update `UI.Components.cs` factory methods for `OmniBox` to accept `OmniBoxOptions`.
3. **Handle Input Logic Based on Mode**
   - In `OnInputChanged` within `Tesserae/src/Components/OmniBox.cs` (which I have verified exists), check the active mode. In `Chat` mode, just reflect the string as plain text and skip the complex formatting logic. In `SearchAndChat`, do similar depending on the active state.
4. **Support Multiline Input**
   - Change `_input` declaration in `Tesserae/src/Components/OmniBox.cs` from `HTMLInputElement` to `HTMLTextAreaElement`. Use `TextArea` helper instead of `TextBox` or create it manually with `_("tss-omnibox-input", type: "text")`.
   - Apply multiline styles in `Tesserae/h5/assets/css/tss.omnibox.css`.
   - Verify `OmniBox.cs` and `tss.omnibox.css` changes via `read_file`.
5. **Mode Selector (Search and Chat)**
   - When mode is `SearchAndChat`, add a selector using `Button` or `ChoiceGroup` at the bottom of the `_container` in `Tesserae/src/Components/OmniBox.cs`.
6. **Header and Footer Methods**
   - Add `SetHeader(IComponent header)`, `SetFooter(IComponent footer)` to `OmniBox` in `Tesserae/src/Components/OmniBox.cs`.
   - Include logic for a model selector on the right side next to the footer, rendering a custom component in the same wrapper container.
7. **Samples Update**
   - Update `Tesserae.Tests/src/Samples/Components/OmniBoxSample.cs` with examples of Chat mode, Search mode, and Search and Chat mode.
   - Add custom nested CSS for styling the dropdown to `Tesserae/h5/assets/css/tss.omnibox.css`.
   - Verify changes via `read_file`.
8. **Build and test locally**
   - Use `dotnet build` in `Tesserae.Tests` to verify compilation.
9. **Pre commit steps**
   - Complete pre commit steps to make sure proper testing, verifications, reviews and reflections are done.
10. **Submit the changes.**
    - Submit using `submit` tool.
