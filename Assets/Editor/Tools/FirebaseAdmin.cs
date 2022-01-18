using System.Threading.Tasks;
using Firebase.Auth;

public static class FirebaseAdmin
{
	static string m_Config;

	public static async Task Login()
	{
		const string email    = "mityugovmaxim@gmail.com";
		const string password = "123456";
		
		if (FirebaseAuth.DefaultInstance.CurrentUser != null && FirebaseAuth.DefaultInstance.CurrentUser.Email == email)
			return;
		
		await FirebaseAuth.DefaultInstance.SignInWithEmailAndPasswordAsync(email, password);
	}

	public static void Logout()
	{
		FirebaseAuth.DefaultInstance.SignOut();
	}
}