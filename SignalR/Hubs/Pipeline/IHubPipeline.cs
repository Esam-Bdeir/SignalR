namespace SignalR.Hubs
{
    /// <summary>
    /// 
    /// </summary>
    public interface IHubPipeline
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        IHubPipeline Use(IHubPipelineBuilder builder);
    }
}
