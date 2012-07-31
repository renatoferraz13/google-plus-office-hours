<?php
/*
 * Copyright 2011 Google Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
if (ini_get('register_globals') === "1") {
 die("register_globals must be turned off before using the starter application");
}

require_once 'google-api-php-client/src/apiClient.php';
require_once 'google-api-php-client/src/contrib/apiPlusService.php';
// History: Fetched from the trunk of the php client library
require_once 'google-api-php-client/src/contrib/apiPlusMomentsService.php';

session_start();

$client = new apiClient();
$client->setApplicationName("Google+ PHP Starter Application");
// Visit https://code.google.com/apis/console to generate your
// oauth2_client_id, oauth2_client_secret, and to register your oauth2_redirect_uri.
$client->setClientId('YOUR_CLIENT_ID');
$client->setClientSecret('YOUR_CLIENT_SECRET');
$client->setRedirectUri('YOUR_REDIRECT_URI');
$client->setDeveloperKey('YOUR_API_KEY');

// Added the plus.moments.write scope
$client->setScopes(array('https://www.googleapis.com/auth/plus.me', 'https://www.googleapis.com/auth/plus.moments.write'));

$plus = new apiPlusService($client);

// History: Create a service object
$history = new apiPlusMomentsService($client);

if (isset($_REQUEST['logout'])) {
  unset($_SESSION['access_token']);
}

if (isset($_GET['code'])) {
  $client->authenticate();
  $_SESSION['access_token'] = $client->getAccessToken();
  $redirect = 'http://' . $_SERVER['HTTP_HOST'] . $_SERVER['PHP_SELF'];
  header('Location: ' . filter_var($redirect, FILTER_SANITIZE_URL));
}

if (isset($_SESSION['access_token'])) {
  $client->setAccessToken($_SESSION['access_token']);
}

// History: Write the moment
if(isset($_POST['moment'])) {
  $target = new ItemScope();
  $target->setUrl("http://bakingdisasters.com/social-blog/final/macaron.html");

  $moment = new Moment();
  $moment->setType("http://schemas.google.com/AddActivity");
  $moment->setTarget($target);
  $moment->setStartDate("2010-06-28T19:00:00-08:00");

  $writtenMoment = $history->moments->insert('me', 'vault', $moment);
}

if ($client->getAccessToken()) {
  $me = $plus->people->get('me');

  // These fields are currently filtered through the PHP sanitize filters.
  // See http://www.php.net/manual/en/filter.filters.sanitize.php
  $url = filter_var($me['url'], FILTER_VALIDATE_URL);
  $img = filter_var($me['image']['url'], FILTER_VALIDATE_URL);
  $name = filter_var($me['displayName'], FILTER_SANITIZE_SPECIAL_CHARS);
  $personMarkup = "<a rel='me' href='$url'>$name</a><div><img src='$img'></div>";

  $optParams = array('maxResults' => 100);
  $activities = $plus->activities->listActivities('me', 'public', $optParams);
  $activityMarkup = '';
  foreach($activities['items'] as $activity) {
    // These fields are currently filtered through the PHP sanitize filters.
    // See http://www.php.net/manual/en/filter.filters.sanitize.php
    $url = filter_var($activity['url'], FILTER_VALIDATE_URL);
    $title = filter_var($activity['title'], FILTER_SANITIZE_STRING, FILTER_FLAG_STRIP_HIGH);
    $content = filter_var($activity['object']['content'], FILTER_SANITIZE_STRING, FILTER_FLAG_STRIP_HIGH);
    $activityMarkup .= "<div class='activity'><a href='$url'>$title</a><div>$content</div></div>";
  }

  // The access token may have been updated lazily.
  $_SESSION['access_token'] = $client->getAccessToken();
} else {
  $authUrl = $client->createAuthUrl();
}
?>
<!doctype html>
<html>
<head><link rel='stylesheet' href='style.css' /></head>
<body>
<header><h1>Google+ Sample App</h1></header>

<!-- History: A button to write a moment -->
<div class="box">

 <h2>Write a moment</h2>
<form method="POST">
  <input type="hidden" name="moment" value="true"/>
  <input type="submit" />
</form>
</div>


<div class="box">

<?php if(isset($personMarkup)): ?>
<div class="me"><?php print $personMarkup ?></div>
<?php endif ?>

<?php if(isset($activityMarkup)): ?>
<div class="activities">Your Activities: <?php print $activityMarkup ?></div>
<?php endif ?>

<?php
  if(isset($authUrl)) {
    print "<a class='login' href='$authUrl'>Connect Me!</a>";
  } else {
   print "<a class='logout' href='?logout'>Logout</a>";
  }
?>
</div>
</body>
</html>
