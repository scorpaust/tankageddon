using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.Core;

public static class AuthenticationWrapper 
{
    public static AuthState AuthState { get; private set; } = AuthState.NotAuthenticated;

    public static async Task<AuthState> DoAuth(int maxTries = 5) 
    {
        if (AuthState == AuthState.Authenticated) return AuthState;

        if (AuthState == AuthState.Authenticating)
        {
            await Authenticating();

            return AuthState;
        }

        await SignInAnonymouslyAsync(maxTries);

        return AuthState;
    }

    private static async Task<AuthState> Authenticating()
    {
        while (AuthState == AuthState.Authenticating || AuthState == AuthState.NotAuthenticated)
        {
            await Task.Delay(200);
        }

        return AuthState;
    }

    private static async Task SignInAnonymouslyAsync(int maxRetries)
    {

        AuthState = AuthState.Authenticating;

        int tries = 0;

        while (AuthState == AuthState.Authenticating && tries < maxRetries)
        {
            try {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

            if (AuthenticationService.Instance.IsSignedIn && AuthenticationService.Instance.IsAuthorized)
            {
                AuthState = AuthState.Authenticated;

                break;
            }
            } catch (AuthenticationException ex)
            {
                Debug.LogError(ex);

                AuthState = AuthState.Error;
            } 
            catch (RequestFailedException exception)
            {
                Debug.LogError(exception);

                AuthState = AuthState.Error;
            }

            tries++;

            await Task.Delay(1000);
        }

        if (AuthState != AuthState.Authenticated)
        {
            Debug.LogWarning($"Player was not signed in successfully after {tries} retries.");
            AuthState = AuthState.TimeOut;
        }
    }
}

public enum AuthState 
{
    NotAuthenticated,
    Authenticating,
    Authenticated,
    Error,
    TimeOut
}
