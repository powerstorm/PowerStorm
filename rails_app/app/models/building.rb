class Building < ActiveRecord::Base
  has_many :meters
  # TODO build up dependencies & stuff
end
