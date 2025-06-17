using Windows.Security.Credentials.UI;
using System.Threading.Tasks;

namespace EMS.Services
{
    public class FingerprintService
    {
        public async Task<bool> AuthenticateMasterAsync()
        {
            try
            {
                // Check if Windows Hello is available
                var availability = await UserConsentVerifier.CheckAvailabilityAsync();
                
                if (availability == UserConsentVerifierAvailability.Available)
                {
                    // Request fingerprint verification
                    var result = await UserConsentVerifier.RequestVerificationAsync(
                        "Please verify your fingerprint for master access");
                    
                    return result == UserConsentVerificationResult.Verified;
                }
                
                return false; // Windows Hello not available
            }
            catch
            {
                return false; // Fingerprint authentication failed
            }
        }

        public async Task<bool> IsFingerprintAvailableAsync()
        {
            try
            {
                var availability = await UserConsentVerifier.CheckAvailabilityAsync();
                return availability == UserConsentVerifierAvailability.Available;
            }
            catch
            {
                return false;
            }
        }
    }
} 