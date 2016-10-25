#pragma once
#include "Card.h"
#include <list>

class Player
{
public:
    Player();
    virtual ~Player();
    virtual void Init(std::list<shared_ptr<Card>> cards);
    virtual void Clear();

    virtual void CardIn(shared_ptr<Card> card);
    virtual shared_ptr<Card> CardOut();
};

