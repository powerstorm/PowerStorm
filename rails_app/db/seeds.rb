# This file should contain all the record creation needed to seed the database with its default values.
# The data can then be loaded with the rake db:seed (or created alongside the db with db:setup).
#
# Examples:
#
#   cities = City.create([{ :name => 'Chicago' }, { :name => 'Copenhagen' }])
#   Mayor.create(:name => 'Daley', :city => cities.first)

Building.create(
 [{:building_name => "Arend",                                   :capacity => 170, :occupants => 0, :abbreviation => "arh", :area => 36900, :building_type => "dorm"},
  {:building_name => "Baldwin Jenkins",                         :capacity => 164, :occupants => 0, :abbreviation => "bjh", :area => 39900, :building_type => "dorm"},
  {:building_name => "Ballard",                                 :capacity => 66,  :occupants => 0, :abbreviation => "bah", :area => 14490, :building_type => "dorm"},
  {:building_name => "Boppell",                                 :capacity => 84,  :occupants => 0, :abbreviation => "boh", :area => 29467, :building_type => "dorm"},
  {:building_name => "Cornerstone",                             :capacity => 24,  :occupants => 0, :abbreviation => "cos", :area => 17086, :building_type => "dorm"},
  {:building_name => "Duvall",                                  :capacity => 162, :occupants => 0, :abbreviation => "duh", :area => 47755, :building_type => "dorm"},
  {:building_name => "East",                                    :capacity => 170, :occupants => 0, :abbreviation => "eah", :area => 40885, :building_type => "dorm"},
  {:building_name => "McMillan",                                :capacity => 85,  :occupants => 0, :abbreviation => "mmh", :area => 20738, :building_type => "dorm"},
  {:building_name => "The Village",                             :capacity => 60,  :occupants => 0, :abbreviation => "thv", :area => 14040, :building_type => "dorm"},
  {:building_name => "Stewart",                                 :capacity => 74,  :occupants => 0, :abbreviation => "sth", :area => 16750, :building_type => "dorm"},
  {:building_name => "Warren",                                  :capacity => 227, :occupants => 0, :abbreviation => "wah", :area => 49400, :building_type => "dorm"},
  {:building_name => "McEachran",                                                                  :abbreviation => "meh",                 :building_type => "other"},
  {:building_name => "Mackay",                                                                     :abbreviation => "mkh",                 :building_type => "other"},
  {:building_name => "Cowles Auditorium",                                                          :abbreviation => "cau",                 :building_type => "other"},
  {:building_name => "Music",                                                                      :abbreviation => "mbl",                 :building_type => "other"},
  {:building_name => "Auld House",                                                                 :abbreviation => "alh",                 :building_type => "other"},
  {:building_name => "Dixon",                                                                      :abbreviation => "dxh",                 :building_type => "other"},
  {:building_name => "Seeley G. Mudd Chapel",                                                      :abbreviation => "sgm",                 :building_type => "other"},
  {:building_name => "Graves Gym",                                                                 :abbreviation => "ggy",                 :building_type => "other"},
  {:building_name => "Fieldhouse",                                                                 :abbreviation => "fho",                 :building_type => "other"},
  {:building_name => "Aquatics Center/Scotford Fitness Center",                                    :abbreviation => "acs",                 :building_type => "other"},
  {:building_name => "Westminster",                                                                :abbreviation => "wmh",                 :building_type => "other"},
  {:building_name => "Weyerhaeuser",                                                               :abbreviation => "whh",                 :building_type => "other"},
  {:building_name => "Lied Center for the Visual Arts",                                            :abbreviation => "lcv",                 :building_type => "other"},
  {:building_name => "Pirates Cove Shelter",                                                       :abbreviation => "pcs",                 :building_type => "other"},
  {:building_name => "Schumacher",                                                                 :abbreviation => "shh",                 :building_type => "other"},
  {:building_name => "Hill House",                                                                 :abbreviation => "hih",                 :building_type => "other"},
  {:building_name => "Facilities Services",                                                        :abbreviation => "fas",                 :building_type => "other"},
  {:building_name => "Robinson Science",                                                           :abbreviation => "rsh",                 :building_type => "other"},
  {:building_name => "Lindaman Center",                                                            :abbreviation => "lic",                 :building_type => "other"},
  {:building_name => "H.C. Cowles Memorial Library",                                               :abbreviation => "hcc",                 :building_type => "other"},
  {:building_name => "Johnston Science Center",                                                    :abbreviation => "jsc",                 :building_type => "other"},
  {:building_name => "Hendrick",                                                                   :abbreviation => "heh",                 :building_type => "other"},
  {:building_name => "Hixson Union Building",                                                      :abbreviation => "hub",                 :building_type => "other"},
  {:building_name => "Hawthorne",                                                                  :abbreviation => "hah",                 :building_type => "other"},
  {:building_name => "President's House",                                                          :abbreviation => "prh",                 :building_type => "other"},
  {:building_name => "Whitworth Presbyterian Church",                                              :abbreviation => "wpc",                 :building_type => "other"}])

User.create(:name => "admin", :password => "password")
