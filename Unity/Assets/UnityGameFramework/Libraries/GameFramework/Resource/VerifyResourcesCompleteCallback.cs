namespace GameFramework.Resource
{
    /// <summary>
    /// 使用可更新模式并校验资源完成时的回调函数。
    /// </summary>
    /// <param name="result">校验资源结果，全部成功为 true，否则为 false。</param>
    public delegate void VerifyResourcesCompleteCallback(bool result);
}
