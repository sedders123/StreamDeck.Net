namespace StreamDeck.Net.Constants
{
    /// <summary>
    /// Contains all possible Events that the <see cref="StreamDeckClient"/> could send.
    /// </summary>
    public static class SentEvents
    {
        ///<summary>
        /// Dynamically change the title of an instance of an action.
        ///</summary>
        public const string SetTitle = "setTitle";

        ///<summary>
        /// Dynamically change the image displayed by an instance of an action.
        ///</summary>
        public const string SetImage = "setImage";

        ///<summary>
        /// Temporarily show an alert icon on the image displayed by an instance of an action.
        ///</summary>
        public const string ShowAlert = "showAlert";

        ///<summary>
        /// Temporarily show an OK checkmark icon on the image displayed by an instance of an action.
        ///</summary>
        public const string ShowOk = "showOk";

        ///<summary>
        /// Save persistent data for the action's instance.
        ///</summary>
        public const string SetSettings = "setSettings";

        ///<summary>
        /// Change the state of the action's instance supporting multiple states.
        ///</summary>
        public const string SetState = "setState";

        ///<summary>
        /// Send a payload to the Property Inspector.
        ///</summary>
        public const string SendToProperyInspector = "sendToPropertyInspector";

        ///<summary>
        /// Switch to one of the preconfigured read-only profiles.
        ///</summary>
        public const string SwitchToProfle = "switchToProfle";

        ///<summary>
        /// Open an URL in the default browser.
        ///</summary>
        public const string OpenUrl = "openUrl";

    }
}
