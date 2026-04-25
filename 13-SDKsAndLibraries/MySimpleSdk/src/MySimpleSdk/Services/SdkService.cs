// filepath: c:\Users\Lucas Jorge\Documents\Default Projects\Back\CSharp-101\MySimpleSdk\MySimpleSdk\src\MySimpleSdk\Services\SdkService.cs
using System;

namespace MySimpleSdk.Services
{
    public class SdkService
    {
        private readonly SdkClient _sdkClient;

        public SdkService(SdkClient sdkClient)
        {
            _sdkClient = sdkClient;
        }

        public async Task<SdkModel> FetchData(int id)
        {
            try
            {
                return await _sdkClient.GetData(id);
            }
            catch (Exception ex)
            {
                throw new SdkException("Error fetching data", ex);
            }
        }

        public async Task<bool> SaveData(SdkModel model)
        {
            try
            {
                return await _sdkClient.PostData(model);
            }
            catch (Exception ex)
            {
                throw new SdkException("Error saving data", ex);
            }
        }
    }
}