namespace Tesserae
{
    [H5.Name("tss.VS")]
    public enum ValidationState
    {
        /// <summary>
        /// This will happen if EVERY registered component has been validated - this will only occur if the User has interacted with all of them OR if a full (re-)validation has been performed
        /// </summary>
        EveryComponentIsValid,

        /// <summary>
        /// This will happen if at least one component has been checked and found to be invalid - it's possible that only a subset of all registered components will have been checked if the User hasn't finished interacting with the form and a full (re-)validation hasn't been performed yet
        /// </summary>
        Invalid,

        /// <summary>
        /// This means that none of the components that have been checked have been found to be invalid - this might mean that the User has only interacted with one registered component so far and THAT is valid while others are not valid yet; they will be checked when the User starts
        /// interacting with them and/or if a full (re-)validation is performed
        /// </summary>
        NoInvalidComponentFoundSoFar
    }
}