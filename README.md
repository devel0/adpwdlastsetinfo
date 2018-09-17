# adpwdlastsetinfo

alert by email when active directory user password going to expire

## scenario

- samba 4 dc
- want to inform IT admin of users with expiring password at least 7 days before sending an email through a [wrapper script](https://github.com/devel0/knowledge/blob/master/linux/send-email-wrapper.md)

## howto

- clone this repo into /opensource then build binaries running `dotnet build` from source subdir `adpwdlastsetinfo`

- create a `/scripts/queryuser` as follow

```sh
#!/bin/bash

if [ "$1" == "" ]; then
	echo "specify user name"
	exit 1
fi

domain="some"

ldapsearch -x -b "dc=$domain,dc=local" -D "CN=ldapquery,CN=Users,DC=$domain,DC=local" -H ldaps://dc.example.com:636 -y /security/dc01/ldapquery "(samaccountname=$1)"
```

where `/security/dc01/ldapquery` contains ldapquery user clear text password ( must 600 mode )

- create a `/scripts/adpwdlastsetinfo` as follow

```sh
#!/bin/bash

dotnet /opensource/adpwdlastsetinfo/adpwdlastsetinfo/bin/Debug/netcoreapp2.1/adpwdlastsetinfo.dll $@
```

- create another script `/scripts/selfcheckpwdexpire` to check against a list of users, specified one by line in a txt file `/security/aduserlist.txt`

```sh
#!/bin/bash

pwexpire=90
expirealertdaysbefore=7
fromaddress="server@example.com"
toaddress="selfcheck@example.com"

tmpfile=/tmp/selfcheckadpwd

neednotify="0"
msg='<table><th><tr><td><b>User</b></td><td><b>Days before expiration</b></td><td><b>Status</b></td></tr></th><tbody>';

while IFS='' read -r line || [[ -n "$line" ]]; do
	echo -n "checking user password expiration [$line]..."
	pwdLastSet="$(/dockers/dc01/query-user "$line" | grep pwdLastSet | awk '{print $2}')"
	echo -n "long=$pwdLastSet..."
	adpwdlastsetinfo $pwdLastSet $pwexpire $expirealertdaysbefore > $tmpfile
	exitcode=$?

	pwdLastSet="$(cat $tmpfile | grep pwdLastSet | sed 's/pwdLastSet:\s//g')"
	daysFromLastSet="$(cat $tmpfile | grep daysFromLastSet | sed 's/daysFromLastSet:\s//g')"
	daysToExpiration="$(cat $tmpfile | grep daysToExpiration | sed 's/daysToExpiration:\s//g')"

	echo "pwdLastSet [$pwdLastSet] ; daysFromLastSet [$daysFromLastSet] ; daysToExpiration [$daysToExpiration]"

	nfo="<b>User</b>: $line<br/>"\
"<b>Last password set</b>: $pwdLastSet<br/>"\
"<b>Days from last set</b>: $daysFromLastSet<br/>"\
"<b>Days before expiration</b>: $daysToExpiration<br/>"


	msg+="<tr><td>$line</td>";

	if [ "$exitcode" == "2" ] ; then
		neednotify="1"
		echo "  --> EXPIRED";
		msg+="<td>0</td><td><font color='red'>EXPIRED</font></td>";
	elif [ "$exitcode" == "3" ]; then
		neednotify="1"
		echo " --> ALERT"
		msg+="<td>$daysToExpiration</td><td><font color='orange'>WARNING</font></td>";
	else
		msg+="<td></td><td>OK</td>"
	fi
	msg+="</tr>"

done < "/security/aduserslist.txt"

msg+='</tbody></table>'

if [ "$neednotify" == "1" ]; then
	/scripts/_sendemail "$fromaddress" "$toaddress" "password expiration summary" "$msg"
fi
```
