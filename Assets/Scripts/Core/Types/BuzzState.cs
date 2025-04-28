namespace PDXUnderground.Core
{
    /// <summary>
    /// Represents the various states of intoxication/buzz for the gambler character
    /// </summary>
    public enum BuzzState
    {
        /// <summary>
        /// No buzz effect
        /// </summary>
        Sober = 0,
        
        /// <summary>
        /// Slight buzz, minor positive effects
        /// </summary>
        Tipsy,
        
        /// <summary>
        /// Medium buzz, moderate positive effects
        /// </summary>
        Buzzed,
        
        /// <summary>
        /// Strong buzz, strong positive effects but some negative
        /// </summary>
        Drunk,
        
        /// <summary>
        /// Extreme buzz, major negative effects
        /// </summary>
        Wasted
    }
}

