namespace StreamDeck.Net.Constants {
    /// <summary>
    /// Contains all possible Events that the <see cref="StreamDeckClient"/> could recieve.
    /// </summary>
    public static class RecievedEvents
    {
        /// <summary>
        /// When the user presses a key, the plugin will receive the keyDown event.
        /// </summary>
        public const string KeyDown = "keyDown";
        /// <summary>
        /// When the user releases a key, the plugin will receive the keyUp event.
        /// </summary>
        public const string KeyUp = "keyUp";
        /// <summary>
        /// When an instance of an action appears, the plugin will receive a willAppear event.
        /// </summary>
        public const string WillAppear = "willAppear";
        /// <summary>
        /// When an instance of an action disappears, the plugin will receive a willDisappear event.
        /// </summary>
        public const string WillDisappear = "willDisappear";
        /// <summary>
        /// When the user changes the title or title parameters, the plugin will receive a titleParametersDidChange event.
        /// </summary>
        public const string TitleParametersDidChange = "titleParametersDidChange";
        /// <summary>
        /// When a device is plugged to the computer, the plugin will receive a deviceDidConnect event.
        /// </summary>
        public const string DeviceDidConnect = "deviceDidConnect";
        /// <summary>
        /// When a device is unplugged from the computer, the plugin will receive a deviceDidDisconnect event.
        /// </summary>
        public const string DeviceDidDisconnect = "deviceDidDisconnect";
        /// <summary>
        /// When a monitored application is launched, the plugin will be notified and will receive the applicationDidLaunch event.
        /// </summary>
        public const string ApplicationDidLaunch = "applicationDidLaunch";
        /// <summary>
        /// When a monitored application is terminated, the plugin will be notified and will receive the applicationDidTerminate event.
        /// </summary>
        public const string ApplicationDidTerminate = "applicationDidTerminate";
    }
}