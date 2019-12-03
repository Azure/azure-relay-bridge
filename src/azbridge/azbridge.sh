#!/bin/bash

export PATH="$PATH:/usr/share/azbridge"

# remove specified host from /etc/hosts

function removehost() {
    if [[ "$1" ]]
    then
        HOSTNAME=$1

        if [ -n "$(grep "\b$HOSTNAME\b" /etc/hosts)" ]
        then
            echo "$HOSTNAME Found in your /etc/hosts, Removing now...";
            sudo sed -i".bak" "/\b$HOSTNAME\b/d" /etc/hosts
        else
            echo "$HOSTNAME was not found in your /etc/hosts";
        fi
    else
        echo "Error: missing required parameters."
        echo "Usage: "
        echo "  removehost domain"
    fi
}

#add new ip host pair to /etc/hosts
function addhost() {
    if [[ "$1" && "$2" ]]
    then
        IP=$1
        HOSTNAME=$2

        if [ -n "$(grep "\b$HOSTNAME\b" /etc/hosts)" ]
            then
                echo "$HOSTNAME already exists:";
                echo $(grep "\b$HOSTNAME\b" /etc/hosts);
            else
                echo "Adding $HOSTNAME to your /etc/hosts";
                printf "%s\t%s\n" "$IP" "$HOSTNAME" | sudo tee -a /etc/hosts > /dev/null;

                if [ -n "$(grep "\b$HOSTNAME\b" /etc/hosts)" ]
                    then
                        echo "$HOSTNAME was added succesfully:";
                        echo $(grep "\b$HOSTNAME\b" /etc/hosts);
                    else
                        echo "Failed to Add $HOSTNAME, Try again!";
                fi
        fi
    else
        echo "Error: missing required parameters."
        echo "Usage: "
        echo "  addhost ip domain"
    fi
}