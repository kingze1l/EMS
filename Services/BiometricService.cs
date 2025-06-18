using Windows.Security.Credentials.UI;
using System.Threading.Tasks;

namespace EMS.Services
{
    public class BiometricService
    {
        public async Task<bool> AuthenticateMasterAsync()
        {
            try
            {
                // Check if Windows Hello is available
                var availability = await UserConsentVerifier.CheckAvailabilityAsync();
                
                if (availability == UserConsentVerifierAvailability.Available)
                {
                    // Request biometric verification (fingerprint or face)
                    var result = await UserConsentVerifier.RequestVerificationAsync(
                        "Please verify your biometric (fingerprint or face) for master access");
                    
                    return result == UserConsentVerificationResult.Verified;
                }
                
                return false; // Windows Hello not available
            }
            catch
            {
                return false; // Biometric authentication failed
            }
        }

        public async Task<bool> IsBiometricAvailableAsync()
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